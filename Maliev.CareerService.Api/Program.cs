using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using FluentValidation;
using HealthChecks.UI.Client;
using Maliev.CareerService.Api.Middleware;
using Maliev.CareerService.Api.Services;
using Maliev.CareerService.Api.Services.External;
using Maliev.CareerService.Data;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Prometheus;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Events;
using StackExchange.Redis;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog - JSON to stdout only
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Service", "CareerService")
    .WriteTo.Console(
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
        formatProvider: null)
    .CreateLogger();

builder.Host.UseSerilog();

try
{
    Log.Information("Starting Maliev Career Service");

    // Load secrets from Google Secret Manager (mounted at /mnt/secrets)
    var secretsPath = "/mnt/secrets";
    if (Directory.Exists(secretsPath))
    {
        builder.Configuration.AddKeyPerFile(directoryPath: secretsPath, optional: true);
    }

    // Redis Distributed Cache Configuration
    var redisConnectionString = builder.Configuration["Redis:ConnectionString"];
    var redisEnabled = bool.TryParse(builder.Configuration["Redis:Enabled"], out var isRedisEnabled) && isRedisEnabled;

    if (redisEnabled && !string.IsNullOrEmpty(redisConnectionString) && !builder.Environment.IsEnvironment("Testing"))
    {
        try
        {
            builder.Services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnectionString;
                options.InstanceName = "Career:";
            });

            var redis = ConnectionMultiplexer.Connect(redisConnectionString);
            builder.Services.AddSingleton<IConnectionMultiplexer>(redis);

            Log.Information("Redis distributed cache configured: {RedisConnectionString}", redisConnectionString);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Redis connection failed - will use in-memory cache fallback");
        }
    }
    else
    {
        Log.Information("Redis disabled or not configured - using in-memory cache only");
    }

    builder.Services.AddMemoryCache(); // Fallback in-memory cache

    // RabbitMQ Configuration (MassTransit)
    var rabbitmqHost = builder.Configuration["RabbitMQ:Host"] ?? "localhost";
    var rabbitmqPort = int.TryParse(builder.Configuration["RabbitMQ:Port"], out var port) ? port : 5672;
    var rabbitmqUser = builder.Configuration["RabbitMQ:Username"] ?? "guest";
    var rabbitmqPassword = builder.Configuration["RabbitMQ:Password"] ?? "guest";
    var rabbitmqVhost = builder.Configuration["RabbitMQ:VirtualHost"] ?? "/";
    var rabbitmqEnabled = bool.TryParse(builder.Configuration["RabbitMQ:Enabled"], out var isRabbitmqEnabled) && isRabbitmqEnabled;

    if (rabbitmqEnabled && !builder.Environment.IsEnvironment("Testing"))
    {
        builder.Services.AddMassTransit(x =>
        {
            // Add consumers here if needed in the future
            // x.AddConsumer<SomeEventConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(rabbitmqHost, (ushort)rabbitmqPort, rabbitmqVhost, h =>
                {
                    h.Username(rabbitmqUser);
                    h.Password(rabbitmqPassword);
                });

                cfg.ConfigureEndpoints(context);
            });
        });

        Log.Information("MassTransit configured with RabbitMQ: {Host}:{Port}", rabbitmqHost, rabbitmqPort);
    }
    else
    {
        Log.Information("RabbitMQ/MassTransit disabled by configuration");
    }

    // Add services to the container
    builder.Services.AddControllers();

    // Add FluentValidation
    builder.Services.AddValidatorsFromAssemblyContaining<Program>();

    // Add AutoMapper
    builder.Services.AddAutoMapper(typeof(Program).Assembly);

    builder.Services.AddEndpointsApiExplorer();

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

    // Configure OpenAPI specification (required for Scalar)
    builder.Services.AddOpenApi();

    // Configure CareerDbContext with PostgreSQL ONLY (no in-memory fallback)
    builder.Services.AddDbContext<CareerDbContext>(options =>
    {
        var connectionString = builder.Configuration.GetConnectionString("CareerDbContext");
        if (!string.IsNullOrEmpty(connectionString))
        {
            options.UseNpgsql(connectionString);
        }
        else if (builder.Environment.IsDevelopment())
        {
            // In development, warn but don't fail - migrations can still be created
            Log.Warning("CareerDbContext connection string not configured. Some features will not work.");
        }
    });

    // Configure Response Caching for read-heavy endpoints
    builder.Services.AddResponseCaching();

    // Register application services
    builder.Services.AddScoped<IMarkdownService, MarkdownService>();
    builder.Services.AddScoped<IJobPostingService, JobPostingService>();
    builder.Services.AddScoped<IApplicationService, ApplicationService>();

    // User Story 2: Training and Learning Services
    builder.Services.AddScoped<ITrainingProgramService, TrainingProgramService>();
    builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
    builder.Services.AddScoped<IELearningResourceService, ELearningResourceService>();

    // User Story 3: Report Service
    builder.Services.AddScoped<IReportService, ReportService>();

    // User Story 4: Development Planning Services
    builder.Services.AddScoped<IDevelopmentPlanService, DevelopmentPlanService>();
    builder.Services.AddScoped<IDevelopmentGoalService, DevelopmentGoalService>();

    // Prometheus Metrics
    builder.Services.AddSingleton<IMetricsService, MetricsService>();

    // Configure External Service Clients with HttpClient
    builder.Services.Configure<EmployeeServiceOptions>(
        builder.Configuration.GetSection("ExternalServices:EmployeeService"));
    builder.Services.AddHttpClient<IEmployeeServiceClient, EmployeeServiceClient>()
        .AddStandardResilienceHandler();

    builder.Services.Configure<UploadServiceOptions>(
        builder.Configuration.GetSection("ExternalServices:UploadService"));
    builder.Services.AddHttpClient<IUploadServiceClient, UploadServiceClient>()
        .AddStandardResilienceHandler();

    builder.Services.Configure<CountryServiceOptions>(
        builder.Configuration.GetSection("ExternalServices:CountryService"));
    builder.Services.AddHttpClient<ICountryServiceClient, CountryServiceClient>()
        .AddStandardResilienceHandler();

    builder.Services.Configure<EmailServiceOptions>(
        builder.Configuration.GetSection("ExternalServices:EmailService"));
    builder.Services.AddHttpClient<IEmailServiceClient, EmailServiceClient>()
        .AddStandardResilienceHandler();

    // Build the application
    var app = builder.Build();

    // Configure middleware pipeline
    app.UseResponseCaching(); // Response caching for read-heavy endpoints
    app.UseHttpMetrics(); // Prometheus HTTP metrics
    app.UseRateLimiter();

    // Authentication & Authorization (only if configured and not in Testing)
    if (!app.Environment.IsEnvironment("Testing"))
    {
        var jwtSection = app.Configuration.GetSection(JwtOptions.SectionName);
        if (jwtSection.Exists())
        {
            app.UseAuthentication();
            app.UseAuthorization();
        }
    }

    // Health check endpoints
    app.MapGet("/careers/liveness", () => "Healthy").AllowAnonymous();
    app.MapHealthChecks("/careers/readiness", new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("readiness"),
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    }).AllowAnonymous();

    // Prometheus metrics endpoint (GET only)
    app.MapMetrics("/careers/metrics").AllowAnonymous().WithMetadata(new HttpMethodMetadata(new[] { "GET" }));

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

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public required string Issuer { get; set; }
    public required string Audience { get; set; }
    public required string PublicKey { get; set; } // Base64-encoded RSA public key from shared config
}

// Make Program class accessible for integration tests
public partial class Program
{ }