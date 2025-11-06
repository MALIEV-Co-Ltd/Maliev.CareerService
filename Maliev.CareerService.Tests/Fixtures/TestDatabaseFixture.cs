using Maliev.CareerService.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Maliev.CareerService.Tests.Fixtures;

/// <summary>
/// Test fixture for managing PostgreSQL test database
/// Uses PostgreSQL connection from appsettings.Testing.json
/// </summary>
public class TestDatabaseFixture : IAsyncLifetime
{
    public string ConnectionString { get; private set; } = string.Empty;
    public bool IsAvailable { get; private set; }
    public string? SkipReason { get; private set; }

    public TestDatabaseFixture()
    {
        // Connection will be initialized in InitializeAsync
    }

    public async Task InitializeAsync()
    {
        // Load configuration from appsettings.Testing.json
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.Testing.json", optional: true)
            .Build();

        ConnectionString = configuration.GetConnectionString("CareerDbContext") ?? string.Empty;

        if (string.IsNullOrWhiteSpace(ConnectionString))
        {
            IsAvailable = false;
            SkipReason = "PostgreSQL not configured. Connection string 'CareerDbContext' not found in appsettings.Testing.json. " +
                        "Integration tests will be skipped.";
            return;
        }

        try
        {
            // Test connection and run migrations
            var optionsBuilder = new DbContextOptionsBuilder<CareerDbContext>();
            optionsBuilder.UseNpgsql(ConnectionString);

            await using var context = new CareerDbContext(optionsBuilder.Options);

            // Ensure database can be created
            await context.Database.EnsureCreatedAsync();

            // Run migrations
            await context.Database.MigrateAsync();

            IsAvailable = true;
        }
        catch (Exception ex)
        {
            IsAvailable = false;
            SkipReason = $"Failed to connect to PostgreSQL: {ex.Message}. " +
                        $"Make sure PostgreSQL is running at: {ConnectionString.Split(';')[0]}";
        }
    }

    public async Task DisposeAsync()
    {
        // Clean up test database if needed
        if (IsAvailable && !string.IsNullOrWhiteSpace(ConnectionString))
        {
            try
            {
                var optionsBuilder = new DbContextOptionsBuilder<CareerDbContext>();
                optionsBuilder.UseNpgsql(ConnectionString);

                await using var context = new CareerDbContext(optionsBuilder.Options);
                // Optionally drop database after tests
                // await context.Database.EnsureDeletedAsync();
            }
            catch
            {
                // Ignore cleanup errors
            }
        }

        await Task.CompletedTask;
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
        optionsBuilder.UseNpgsql(ConnectionString);
        return new CareerDbContext(optionsBuilder.Options);
    }
}
