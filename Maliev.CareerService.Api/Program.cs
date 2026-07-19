using Maliev.Aspire.ServiceDefaults;
using Maliev.CareerService.Application.Services;
using Maliev.CareerService.Application.Services.External;
using Maliev.CareerService.Infrastructure.Services;
using Maliev.CareerService.Infrastructure.Services.External;
using Maliev.CareerService.Infrastructure.Data;
using MassTransit;
// Initialize bootstrap logging
using var loggerFactory = LoggerFactory.Create(logBuilder => logBuilder.AddConsole());
var bootstrapLogger = loggerFactory.CreateLogger("Program");

try
{
    Program.Log.StartingHost(bootstrapLogger, "Career Service");

    var builder = WebApplication.CreateBuilder(args);

    // --- Secrets & Configuration ---
    builder.AddGoogleSecretManagerVolume(); // Load secrets from /mnt/secrets if available

    // --- Infrastructure & Observability ---
    builder.AddServiceDefaults(); // OpenTelemetry, health checks, resilience
    builder.AddStandardMiddleware(options =>
    {
        options.EnableRequestLogging = true;
    });
    builder.AddServiceMeters("careers-meter"); // Register service meters for OpenTelemetry business metrics

    builder.AddStandardCache("career:"); // Redis + in-memory fallback, memory-optimized // Redis with in-memory fallback
    builder.AddMassTransitWithRabbitMq(x =>
    {
        x.AddEntityFrameworkOutbox<CareerDbContext>(o =>
        {
            o.UsePostgres();
            o.UseBusOutbox();
        });

        x.AddConsumer<Maliev.CareerService.Api.Consumers.EmployeeCreatedEventConsumer>();
        x.AddConsumer<Maliev.CareerService.Api.Consumers.EmployeeTerminatedEventConsumer>();
    });
    builder.AddPostgresDbContext<CareerDbContext>(connectionName: "CareerDbContext"); // PostgreSQL with retry logic

    // --- API Configuration ---
    builder.AddStandardCors(); // CORS with fail-fast validation
    builder.AddDefaultApiVersioning(); // API versioning with URL segment reader

    // JWT Authentication (tests override via PostConfigureAll with dynamic RSA keys)
    builder.AddJwtAuthentication();

    // Permission-based Authorization
    builder.Services.AddPermissionAuthorization();

    // Add OpenAPI (must be in Program.cs for XML comments to work via source generator)
    if (!builder.Environment.IsProduction())
    {
        builder.AddStandardOpenApi(
            title: "MALIEV Career Service API",
            description: "Human resources and career development service. Manages job postings with search and filtering, job applications with status tracking, employee training programs and enrollments, e-learning resources, individual development plans and goals, and HR analytics reports.");
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

    // Feature 003: Training Records and Skills Migration
    builder.Services.AddScoped<ITrainingRecordService, TrainingRecordService>();
    builder.Services.AddScoped<IEmployeeSkillService, EmployeeSkillService>();
    builder.Services.AddScoped<IMandatoryTrainingService, MandatoryTrainingService>();
    builder.Services.AddHostedService<Maliev.CareerService.Api.BackgroundServices.CertificationExpirationReminderBackgroundService>();
    builder.Services.AddHostedService<Maliev.CareerService.Api.BackgroundServices.OverdueTrainingEscalationBackgroundService>();

    // User Story 3: Report Service
    builder.Services.AddScoped<IReportService, ReportService>();

    // User Story 4: Development Planning Services
    builder.Services.AddScoped<IDevelopmentPlanService, DevelopmentPlanService>();
    builder.Services.AddScoped<IDevelopmentGoalService, DevelopmentGoalService>();

    // Prometheus Metrics
    builder.Services.AddSingleton<IMetricsService, MetricsService>();

    // IAM Registration
    builder.AddIAMServiceClient("career");
    builder.Services.AddIAMRegistration<CareerIAMRegistrationService>("career");

    builder.AddServiceClient<IEmployeeServiceClient, EmployeeServiceClient>("EmployeeService");
    builder.AddServiceClient<IUploadServiceClient, UploadServiceClient>("UploadService");
    builder.AddServiceClient<ICountryServiceClient, CountryServiceClient>("CountryService");
    builder.AddServiceClient<IEmailServiceClient, EmailServiceClient>("NotificationService");
    builder.AddServiceClient<INotificationServiceClient, NotificationServiceClient>("NotificationService");


    // Configure Rate Limiting
    builder.AddStandardRateLimiting(); // Memory-optimized for low-spec nodes
    builder.Services.AddControllers();

    var app = builder.Build();
    var logger = app.Services.GetRequiredService<ILogger<Program>>();

    // --- Database Migrations ---
    // AppHost system tests also run with Testing, so the service must own schema creation.
    await app.MigrateDatabaseAsync<CareerDbContext>();

    // Configure middleware pipeline
    app.UseStandardMiddleware();
    if (!app.Environment.IsDevelopment())
    {
        app.UseHttpsRedirection();
    }
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

    Program.Log.ServiceStarted(logger, "Career Service");
    await app.RunAsync();
}
catch (Exception ex)
{
    Program.Log.HostTerminated(bootstrapLogger, ex, "Career Service");
    throw;
}
finally
{
    loggerFactory.Dispose();
}

/// <summary>
/// Main program class for the application
/// </summary>
public partial class Program
{
    internal static partial class Log
    {
        [LoggerMessage(Level = LogLevel.Information, Message = "Starting {ServiceName} host")]
        public static partial void StartingHost(ILogger logger, string serviceName);

        [LoggerMessage(Level = LogLevel.Critical, Message = "{ServiceName} host terminated unexpectedly during startup")]
        public static partial void HostTerminated(ILogger logger, Exception ex, string serviceName);

        [LoggerMessage(Level = LogLevel.Information, Message = "{ServiceName} started successfully")]
        public static partial void ServiceStarted(ILogger logger, string serviceName);
    }
}
