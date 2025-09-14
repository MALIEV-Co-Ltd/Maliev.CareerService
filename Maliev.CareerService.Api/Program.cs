using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using HealthChecks.UI.Client;
using Maliev.CareerService.Api.Configurations;
using Maliev.CareerService.Api.HealthChecks;
using Maliev.CareerService.Api.Middleware;
using Maliev.CareerService.Api.Models;
using Maliev.CareerService.Api.Services;
using Maliev.CareerService.Data.DbContexts;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Prometheus;
using Serilog;
using Serilog.Filters;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text; 
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithEnvironmentName()
    .Enrich.WithMachineName()
    .Enrich.WithProcessId()
    .Enrich.WithThreadId()
    .Filter.ByExcluding(Matching.WithProperty<string>("RequestPath", path =>
        path.StartsWith("/health") || path.StartsWith("/metrics")))
    .WriteTo.Console(outputTemplate:
        "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {CorrelationId} {SourceContext} {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/career-service-.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 31,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {CorrelationId} {SourceContext} {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();

try
{
    Log.Information("Starting Maliev Career Service");

    // Load secrets.yaml
    builder.Configuration.AddYamlFile("secrets.yaml", optional: true, reloadOnChange: true);

    // Load secrets from mounted volume in GKE
    var secretsPath = "/mnt/secrets";
    if (Directory.Exists(secretsPath))
    {
        builder.Configuration.AddKeyPerFile(directoryPath: secretsPath, optional: true);
    }

    // Add services to the container
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddOpenApi();
    
    // API Versioning
    builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
        options.ApiVersionReader = new UrlSegmentApiVersionReader();
    }).AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

    builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
    builder.Services.AddSwaggerGen();

    // Configure strongly-typed configuration options with validation
    builder.Services.Configure<RateLimitOptions>(builder.Configuration.GetSection(RateLimitOptions.SectionName));
    builder.Services.Configure<CacheOptions>(builder.Configuration.GetSection(CacheOptions.SectionName));
    builder.Services.Configure<UploadServiceOptions>(builder.Configuration.GetSection(UploadServiceOptions.SectionName));
    builder.Services.Configure<GcsConfiguration>(builder.Configuration.GetSection(GcsConfiguration.SectionName));
    builder.Services.Configure<CorsOptions>(builder.Configuration.GetSection(CorsOptions.SectionName));

    // Configure JWT options only if available (to allow local development without secrets)
    var jwtSection = builder.Configuration.GetSection(JwtOptions.SectionName);
    if (!string.IsNullOrEmpty(jwtSection["Issuer"]) && !builder.Environment.IsEnvironment("Testing"))
    {
        builder.Services.Configure<JwtOptions>(jwtSection);
        builder.Services.AddOptions<JwtOptions>()
            .Bind(jwtSection)
            .ValidateDataAnnotations()
            .ValidateOnStart();
    }

    builder.Services.AddOptions<RateLimitOptions>()
        .Bind(builder.Configuration.GetSection(RateLimitOptions.SectionName))
        .ValidateDataAnnotations();

    // Configure Cache options - register both as IOptions<T> and as concrete type for DI
    builder.Services.Configure<CacheOptions>(builder.Configuration.GetSection(CacheOptions.SectionName));
    builder.Services.AddSingleton<CacheOptions>(provider =>
        provider.GetRequiredService<IOptions<CacheOptions>>().Value);

    // Configure service options with fallbacks for Development
    if (builder.Environment.IsDevelopment())
    {
        // Provide default configurations for local development
        builder.Services.Configure<UploadServiceOptions>(options =>
        {
            options.BaseUrl = "http://localhost:8080";
            options.TimeoutSeconds = 30;
        });
        builder.Services.Configure<GcsConfiguration>(options =>
        {
            options.BasePath = "careers";
        });
    }
    else
    {
        builder.Services.AddOptions<UploadServiceOptions>()
            .Bind(builder.Configuration.GetSection(UploadServiceOptions.SectionName))
            .ValidateDataAnnotations();

        builder.Services.AddOptions<GcsConfiguration>()
            .Bind(builder.Configuration.GetSection(GcsConfiguration.SectionName))
            .ValidateDataAnnotations();
    }

    // Configure Career DbContext
    if (builder.Environment.IsEnvironment("Testing") || builder.Environment.IsDevelopment())
    {
        // Use in-memory database for Testing and Development (when no connection string available)
        var connectionString = builder.Configuration.GetConnectionString("CareerDbContext");
        if (string.IsNullOrEmpty(connectionString))
        {
            builder.Services.AddDbContext<CareerDbContext>(options =>
                options.UseInMemoryDatabase(builder.Environment.IsDevelopment() ? "DevDb" : "TestDb"));
        }
        else
        {
            builder.Services.AddDbContext<CareerDbContext>(options =>
                options.UseNpgsql(connectionString));
        }
    }
    else
    {
        builder.Services.AddDbContext<CareerDbContext>(options =>
        {
            options.UseNpgsql(builder.Configuration.GetConnectionString("CareerDbContext"));
        });
    }

    builder.Services.AddDatabaseDeveloperPageExceptionFilter();

    // Configure Memory Cache
    builder.Services.AddMemoryCache(options =>
    {
        var cacheOptions = new CacheOptions();
        builder.Configuration.GetSection(CacheOptions.SectionName).Bind(cacheOptions);
        options.SizeLimit = cacheOptions.MaxCacheSize;
    });

    // Configure HTTP client for UploadService
    builder.Services.AddHttpClient<IUploadServiceClient, UploadServiceClient>((serviceProvider, client) =>
    {
        var uploadServiceOptions = serviceProvider.GetRequiredService<IOptions<UploadServiceOptions>>().Value;
        client.BaseAddress = new Uri(uploadServiceOptions.BaseUrl);
        client.Timeout = TimeSpan.FromSeconds(uploadServiceOptions.TimeoutSeconds);
    });

    // Configure HTTP client for UploadService health checks
    builder.Services.AddHttpClient<UploadServiceHealthCheck>((serviceProvider, client) =>
    {
        var uploadServiceOptions = serviceProvider.GetRequiredService<IOptions<UploadServiceOptions>>().Value;
        client.BaseAddress = new Uri(uploadServiceOptions.BaseUrl);
        client.Timeout = TimeSpan.FromSeconds(10); // Short timeout for health checks
    });

    // Register application services
    builder.Services.AddScoped<IJobPositionService, JobPositionService>();
    builder.Services.AddScoped<IJobApplicationService, JobApplicationService>();
    builder.Services.AddScoped<IWorkLocationService, WorkLocationService>();
    builder.Services.AddScoped<ISkillService, SkillService>();
    builder.Services.AddScoped<IDocumentService, DocumentService>();

    // Configure Rate Limiting
    builder.Services.AddRateLimiter(options =>
    {
        var rateLimitOptions = new RateLimitOptions 
        { 
            CareerEndpoint = new RateLimitOptions.CareerEndpointOptions(),
            Global = new RateLimitOptions.GlobalOptions()
        };
        builder.Configuration.GetSection(RateLimitOptions.SectionName).Bind(rateLimitOptions);

        // Career endpoint rate limiting
        options.AddPolicy("CareerPolicy", context =>
            RateLimitPartition.GetSlidingWindowLimiter(
                partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                factory: _ => new SlidingWindowRateLimiterOptions
                {
                    PermitLimit = rateLimitOptions.CareerEndpoint.PermitLimit,
                    Window = rateLimitOptions.CareerEndpoint.Window,
                    SegmentsPerWindow = 2,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = rateLimitOptions.CareerEndpoint.QueueLimit
                }));

        // Global rate limiting
        options.AddPolicy("GlobalPolicy", context =>
            RateLimitPartition.GetSlidingWindowLimiter(
                partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                factory: _ => new SlidingWindowRateLimiterOptions
                {
                    PermitLimit = rateLimitOptions.Global.PermitLimit,
                    Window = rateLimitOptions.Global.Window,
                    SegmentsPerWindow = 4,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = rateLimitOptions.Global.QueueLimit
                }));

        options.OnRejected = async (context, token) =>
        {
            context.HttpContext.Response.StatusCode = 429;
            await context.HttpContext.Response.WriteAsync("Rate limit exceeded. Please try again later.", token);
        };
    });

    // Configure CORS
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(
            policy =>
            {
                var corsOptions = new CorsOptions();
                builder.Configuration.GetSection(CorsOptions.SectionName).Bind(corsOptions);
                
                policy.WithOrigins(corsOptions.AllowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod();
            });
    });

    // Configure JWT Authentication (skip in Testing environment)
    if (!builder.Environment.IsEnvironment("Testing"))
    {
        if (jwtSection.Exists())
        {
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                var jwtOptions = new JwtOptions
                {
                    Issuer = "default-issuer",
                    Audience = "default-audience", 
                    SecurityKey = "default-key"
                };
                jwtSection.Bind(jwtOptions);

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecurityKey))
                };
            });
        }
        else
        {
            // Log warning that JWT is not configured for local development
            Log.Warning("JWT configuration not found - API will start but authentication will not work. Configure JWT secrets for full functionality.");
        }
    }

    builder.Services.AddAuthorization();

    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders =
            ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        options.KnownNetworks.Clear();
        options.KnownProxies.Clear();
    });

    builder.Services.AddHealthChecks()
        .AddDbContextCheck<CareerDbContext>("CareerDbContext", tags: new[] { "readiness" })
        .AddCheck<DatabaseHealthCheck>("Database Health Check", tags: new[] { "readiness" })
        .AddCheck<UploadServiceHealthCheck>("UploadService Health Check", tags: new[] { "readiness" });

    var app = builder.Build();

    app.UseForwardedHeaders();

    // Add correlation ID middleware early in pipeline
    app.UseCorrelationId();

    // Configure the HTTP request pipeline
    app.UseSwagger(c => 
    {
        c.RouteTemplate = "careers/swagger/{documentName}/swagger.json";
    });
    app.UseSwaggerUI(c =>
    {
        var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
        foreach (var description in provider.ApiVersionDescriptions)
        {
            c.SwaggerEndpoint($"/careers/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
        }
        c.RoutePrefix = "careers/swagger";
    });

    app.UseMiddleware<ExceptionHandlingMiddleware>();
    app.UseHttpsRedirection();
    app.UseHttpMetrics();

    app.UseRateLimiter();
    app.UseCors();
    
    // JWT Authentication & Authorization (only if configured and not in Testing environment)
    if (!app.Environment.IsEnvironment("Testing"))
    {
        var appJwtSection = app.Configuration.GetSection(JwtOptions.SectionName);
        if (appJwtSection.Exists())
        {
            app.UseAuthentication();
            app.UseAuthorization();
        }
    }

    // Health check endpoints (allow anonymous access for monitoring)
    app.MapGet("/careers/liveness", () => "Healthy").AllowAnonymous();

    app.MapHealthChecks("/careers/readiness", new HealthCheckOptions
    {
        Predicate = healthCheck => healthCheck.Tags.Contains("readiness"),
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    }).AllowAnonymous();

    app.MapMetrics("/careers/metrics");
    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// Make Program class accessible for integration tests
public partial class Program
{ }