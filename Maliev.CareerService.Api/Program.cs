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
using Serilog;
using Serilog.Filters;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text; 
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

try
{
    Log.Information("Starting Maliev Career Service");

    // Load secrets from multiple sources for flexibility across environments
    // 1. YAML file (if exists)
    builder.Configuration.AddYamlFile("secrets.yaml", optional: true, reloadOnChange: true);

    // 2. Environment variables (highest priority)
    builder.Configuration.AddEnvironmentVariables();

    // 3. Key-per-file from mounted volume (for containerized environments)
    var secretsPath = Environment.GetEnvironmentVariable("SECRETS_PATH") ?? "/mnt/secrets";
    if (Directory.Exists(secretsPath))
    {
        builder.Configuration.AddKeyPerFile(directoryPath: secretsPath, optional: true);
    }

    // Add services to the container
    builder.Services.AddControllers(options =>
    {
        options.Filters.Add<Maliev.CareerService.Api.Filters.ValidationFilter>();
    });
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

    // Configure Redis caching
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = builder.Configuration.GetConnectionString("Redis");
        options.InstanceName = "MalievCareerService_";
    });
    
    // Register Redis connection multiplexer for health checks
    builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    {
        var connectionString = builder.Configuration.GetConnectionString("Redis");
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Redis connection string is not configured");
        }
        return ConnectionMultiplexer.Connect(connectionString);
    });

    // Register Redis cache service
    builder.Services.AddScoped<IRedisCacheService, RedisCacheService>();

    builder.Services.AddHealthChecks();
        
    // Register fallback cache service
    builder.Services.AddScoped<IFallbackCacheService, FallbackCacheService>();
    
    // Register cache versioning service
    builder.Services.AddScoped<ICacheVersioningService, CacheVersioningService>();

    // Configure service options with fallbacks for Testing only
    if (builder.Environment.IsEnvironment("Testing"))
    {
        // Provide default configurations for testing environment only
        builder.Services.Configure<UploadServiceOptions>(options =>
        {
            options.BaseUrl = "http://localhost:8080";
            options.TimeoutSeconds = 30;
        });
        builder.Services.Configure<GcsConfiguration>(options =>
        {
            options.BasePath = "careers";
        });

        // Add validation for development (skip validation for testing to avoid startup issues)
        if (builder.Environment.IsDevelopment())
        {
            builder.Services.AddOptions<UploadServiceOptions>()
                .ValidateDataAnnotations()
                .ValidateOnStart();

            builder.Services.AddOptions<GcsConfiguration>()
                .PostConfigure(options =>
                {
                    // Ensure defaults are applied if config section is empty
                    if (string.IsNullOrEmpty(options.BasePath))
                    {
                        options.BasePath = "careers";
                    }
                })
                .ValidateDataAnnotations()
                .ValidateOnStart();
        }
    }
    else
    {
        builder.Services.AddOptions<UploadServiceOptions>()
            .Bind(builder.Configuration.GetSection(UploadServiceOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        builder.Services.AddOptions<GcsConfiguration>()
            .Bind(builder.Configuration.GetSection(GcsConfiguration.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
    }

    // Configure Career DbContext
    if (builder.Environment.IsEnvironment("Testing") || builder.Environment.IsDevelopment())
    {
        // Use in-memory database for Testing and Development (when no connection string available)
        var connectionString = builder.Configuration.GetConnectionString("CareerDbContext");
        if (string.IsNullOrEmpty(connectionString))
        {
            builder.Services.AddDbContext<CareerDbContext>(options =>
                options.UseInMemoryDatabase("TestDb"));
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

    // Add Database Developer Page Exception Filter only in Development environment
    if (builder.Environment.IsDevelopment())
    {
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();
    }

    // Configure Memory Cache (simple configuration without SizeLimit)
    builder.Services.AddMemoryCache();

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
    builder.Services.AddScoped<IBusinessEventLogger, BusinessEventLogger>();
    builder.Services.AddScoped<IFileValidationService, FileValidationService>();
    builder.Services.AddScoped<ICacheInvalidationService, CacheInvalidationService>();
    builder.Services.AddHostedService<CacheWarmingService>();

    // Configure Rate Limiting
    builder.Services.AddRateLimiter(options =>
    {
        var rateLimitOptions = new RateLimitOptions 
        { 
            CareerEndpoint = new RateLimitOptions.CareerEndpointOptions(),
            Global = new RateLimitOptions.GlobalOptions(),
            User = new RateLimitOptions.UserOptions()
        };
        builder.Configuration.GetSection(RateLimitOptions.SectionName).Bind(rateLimitOptions);

        // Career endpoint rate limiting
        options.AddPolicy("CareerPolicy", context =>
        {
            // Get client IP considering proxies
            var clientIP = context.Request.Headers["X-Forwarded-For"].FirstOrDefault() 
                          ?? context.Connection.RemoteIpAddress?.ToString() 
                          ?? "unknown";
                          
            return RateLimitPartition.GetSlidingWindowLimiter(
                partitionKey: clientIP,
                factory: _ => new SlidingWindowRateLimiterOptions
                {
                    PermitLimit = rateLimitOptions.CareerEndpoint.PermitLimit,
                    Window = rateLimitOptions.CareerEndpoint.Window,
                    SegmentsPerWindow = 2,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = rateLimitOptions.CareerEndpoint.QueueLimit
                });
        });

        // Global rate limiting
        options.AddPolicy("GlobalPolicy", context =>
        {
            // Get client IP considering proxies
            var clientIP = context.Request.Headers["X-Forwarded-For"].FirstOrDefault() 
                          ?? context.Connection.RemoteIpAddress?.ToString() 
                          ?? "unknown";
                          
            return RateLimitPartition.GetSlidingWindowLimiter(
                partitionKey: clientIP,
                factory: _ => new SlidingWindowRateLimiterOptions
                {
                    PermitLimit = rateLimitOptions.Global.PermitLimit,
                    Window = rateLimitOptions.Global.Window,
                    SegmentsPerWindow = 4,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = rateLimitOptions.Global.QueueLimit
                });
        });
        
        // User-based rate limiting (for authenticated users)
        options.AddPolicy("UserPolicy", context =>
        {
            // Get user ID from claims, fallback to IP
            var userId = context.User?.Identity?.IsAuthenticated == true 
                ? context.User.FindFirst("sub")?.Value 
                : context.Request.Headers["X-Forwarded-For"].FirstOrDefault() 
                  ?? context.Connection.RemoteIpAddress?.ToString() 
                  ?? "unknown";
                          
            return RateLimitPartition.GetSlidingWindowLimiter(
                partitionKey: userId,
                factory: _ => new SlidingWindowRateLimiterOptions
                {
                    PermitLimit = rateLimitOptions.User.PermitLimit,
                    Window = rateLimitOptions.User.Window,
                    SegmentsPerWindow = 3,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = rateLimitOptions.User.QueueLimit
                });
        });

        options.OnRejected = async (context, token) =>
        {
            context.HttpContext.Response.StatusCode = 429;
            await context.HttpContext.Response.WriteAsync("Rate limit exceeded. Please try again later.", token);
            
            // Log rate limit exceeded event as a security event
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            var businessEventLogger = context.HttpContext.RequestServices.GetRequiredService<IBusinessEventLogger>();
            var clientIP = context.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            
            businessEventLogger.LogSecurityEvent(
                "RateLimitExceeded", 
                $"Rate limit exceeded for client {clientIP}", 
                new { ClientIP = clientIP, Path = context.HttpContext.Request.Path });
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
    
    // Configure request size limits
    builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
    {
        options.MultipartBodyLengthLimit = 104857600; // 100MB
    });

    builder.Services.AddHealthChecks()
        .AddDbContextCheck<CareerDbContext>("CareerDbContext", tags: new[] { "readiness" })
        .AddCheck<DatabaseHealthCheck>("Database Health Check", tags: new[] { "readiness" })
        .AddCheck<UploadServiceHealthCheck>("UploadService Health Check", tags: new[] { "readiness" })
        .AddCheck<GcsHealthCheck>("GCS Health Check", tags: new[] { "readiness" })
        .AddCheck<MemoryHealthCheck>("Memory Health Check", tags: new[] { "readiness", "liveness" })
        .AddCheck<ResponseTimeHealthCheck>("Response Time Health Check", tags: new[] { "readiness" })
        ;
        // Redis health check removed - using in-memory cache only

    var app = builder.Build();

    app.UseForwardedHeaders();

    // Add response time monitoring early in pipeline
    app.UseResponseTimeMonitoring();

    // Add correlation ID middleware early in pipeline
    app.UseCorrelationId();
    
    // Add logging context middleware to enrich all logs with request information
    app.UseLoggingContext();
    
    // Add security headers and input sanitization
    app.UseSecurityHeaders();

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