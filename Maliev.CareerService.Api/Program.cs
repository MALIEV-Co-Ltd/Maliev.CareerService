using Maliev.CareerService.Api.Services;
using Maliev.CareerService.Api.Services.External;
using Maliev.CareerService.Data;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// --- Secrets & Configuration ---
builder.AddGoogleSecretManagerVolume(); // Load secrets from /mnt/secrets if available

// --- Infrastructure & Observability ---
builder.AddServiceDefaults(); // OpenTelemetry, health checks, resilience
builder.AddServiceMeters("careers-meter"); // Register service meters for OpenTelemetry business metrics

builder.AddRedisDistributedCache(instanceName: "career:"); // Redis with in-memory fallback
builder.AddMassTransitWithRabbitMq(); // RabbitMQ message bus (non-blocking startup)
builder.AddPostgresDbContext<CareerDbContext>(connectionStringName: "CareerDbContext"); // PostgreSQL with retry logic

// --- API Configuration ---
builder.AddDefaultCors(); // CORS from CORS:AllowedOrigins config
builder.AddDefaultApiVersioning(); // API versioning with URL segment reader

// JWT Authentication (tests override via PostConfigureAll with dynamic RSA keys)
builder.AddJwtAuthentication();

// Add OpenAPI (must be in Program.cs for XML comments to work via source generator)
if (!builder.Environment.IsProduction())
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddOpenApi("v1", options =>
    {
        options.AddDocumentTransformer((document, context, cancellationToken) =>
        {
            document.Info.Title = "MALIEV Career Service API";
            document.Info.Version = "v1";
            document.Info.Description = "Human resources and career development service. Manages job postings with search and filtering, job applications with status tracking, employee training programs and enrollments, e-learning resources, individual development plans and goals, and HR analytics reports.";
            return Task.CompletedTask;
        });
    });
}

// Configure Response Caching for read-heavy endpoints
builder.Services.AddResponseCaching();
builder.Services.AddMemoryCache();

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

// Configure Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    // Global rate limiter for authenticated users
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)
            }));

    // Named policy for anonymous/public endpoints
    options.AddPolicy("anonymous", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 50,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)
            }));
});

builder.Services.AddControllers();

var app = builder.Build();
var logger = app.Services.GetRequiredService<ILogger<Program>>();

// Run database migrations on startup (skip in Testing environment)
if (!app.Environment.IsEnvironment("Testing"))
{
    try
    {
        await app.MigrateDatabaseAsync<CareerDbContext>();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Database migration failed - application may not function correctly");
        // Don't throw - allow app to start for debugging
    }
}

// Configure middleware pipeline
app.UseHttpsRedirection();
app.UseResponseCaching(); // Response caching for read-heavy endpoints
app.UseRateLimiter();
app.UseCors();

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Map endpoints after middleware
app.MapControllers();

// Map Aspire default endpoints (/health, /alive, /metrics)
app.MapDefaultEndpoints(servicePrefix: "career");

// Map OpenAPI and Scalar documentation (dev/staging only)
app.MapApiDocumentation(servicePrefix: "career");

logger.LogInformation("CareerService started successfully");
await app.RunAsync();

/// <summary>
/// Main program class for the application
/// </summary>
public partial class Program { }
