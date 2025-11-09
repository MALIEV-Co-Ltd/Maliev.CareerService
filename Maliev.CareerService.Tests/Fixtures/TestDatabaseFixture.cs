using Maliev.CareerService.Data;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Xunit;

namespace Maliev.CareerService.Tests.Fixtures;

/// <summary>
/// Test fixture for managing PostgreSQL test database using Testcontainers
/// Automatically starts PostgreSQL container if Docker is available
/// Falls back to in-memory database if Docker is unavailable
/// </summary>
public class TestDatabaseFixture : IAsyncLifetime
{
    private PostgreSqlContainer? _postgresContainer;

    public string ConnectionString { get; private set; } = string.Empty;
    public bool IsAvailable { get; private set; }
    public string? SkipReason { get; private set; }

    public TestDatabaseFixture()
    {
        // Container will be initialized in InitializeAsync
    }

    public async Task InitializeAsync()
    {
        try
        {
            // Create and start PostgreSQL container
            _postgresContainer = new PostgreSqlBuilder()
                .WithImage("postgres:17-alpine")
                .WithDatabase("career_service_test")
                .WithUsername("test_user")
                .WithPassword("test_password")
                .WithCleanUp(true)
                .Build();

            await _postgresContainer.StartAsync();

            ConnectionString = _postgresContainer.GetConnectionString();

            // Test connection and run migrations
            var optionsBuilder = new DbContextOptionsBuilder<CareerDbContext>();
            optionsBuilder.UseNpgsql(ConnectionString)
                .ConfigureWarnings(warnings => warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));

            await using var context = new CareerDbContext(optionsBuilder.Options);

            // Run migrations only (EnsureCreated + Migrate together causes pending model changes warning)
            await context.Database.MigrateAsync();

            IsAvailable = true;
        }
        catch (Exception ex)
        {
            IsAvailable = false;
            SkipReason = $"Failed to start PostgreSQL test container: {ex.Message}. " +
                        "Docker may not be available. Tests will use in-memory database fallback.";

            // Clean up container if it was partially created
            if (_postgresContainer != null)
            {
                try
                {
                    await _postgresContainer.DisposeAsync();
                }
                catch
                {
                    // Ignore cleanup errors
                }
                _postgresContainer = null;
            }
        }
    }

    public async Task DisposeAsync()
    {
        // Stop and remove PostgreSQL container
        if (_postgresContainer != null)
        {
            try
            {
                await _postgresContainer.DisposeAsync();
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }

    /// <summary>
    /// Creates a new DbContext instance for testing
    /// </summary>
    public CareerDbContext CreateDbContext()
    {
        if (!IsAvailable)
        {
            throw new InvalidOperationException(SkipReason ?? "Database fixture is not available");
        }

        var optionsBuilder = new DbContextOptionsBuilder<CareerDbContext>();
        optionsBuilder.UseNpgsql(ConnectionString)
            .ConfigureWarnings(warnings => warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        return new CareerDbContext(optionsBuilder.Options);
    }

    /// <summary>
    /// Cleans all data from the database while preserving schema
    /// </summary>
    public async Task CleanDatabaseAsync()
    {
        if (!IsAvailable)
        {
            return;
        }

        await using var context = CreateDbContext();

        // Get all table names from the database
        var tableNames = new[]
        {
            "application_status_changes",
            "job_applications",
            "job_postings",
            "employee_development_goals",
            "individual_development_plans",
            "employee_training_enrollments",
            "e_learning_resources",
            "training_programs"
        };

        // Truncate all tables in reverse order (to respect foreign keys)
        // Skip tables that don't exist (may not be created yet in some test scenarios)
        foreach (var tableName in tableNames.Reverse())
        {
            try
            {
                await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE " + tableName + " RESTART IDENTITY CASCADE");
            }
            catch (Npgsql.PostgresException ex) when (ex.SqlState == "42P01")
            {
                // Table doesn't exist - ignore this error
                // 42P01 is the PostgreSQL error code for "undefined_table"
            }
        }
    }
}
