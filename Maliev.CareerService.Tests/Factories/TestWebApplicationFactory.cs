using Maliev.CareerService.Data;
using Maliev.CareerService.Tests.Fixtures;
using Maliev.CareerService.Tests.Helpers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace Maliev.CareerService.Tests.Factories;

/// <summary>
/// Test web application factory for integration tests
/// </summary>
public class TestWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly TestDatabaseFixture _databaseFixture = new();

    public bool IsDockerAvailable => _databaseFixture.IsAvailable;
    public string? SkipReason => _databaseFixture.SkipReason;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Always configure authentication for tests (even if database unavailable)
            services.AddAuthentication(TestAuthHandler.AuthenticationScheme)
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                    TestAuthHandler.AuthenticationScheme, options => { });

            // Remove the existing DbContext registration
            services.RemoveAll<DbContextOptions<CareerDbContext>>();
            services.RemoveAll<CareerDbContext>();

            // Configure DbContext based on PostgreSQL availability
            if (_databaseFixture.IsAvailable)
            {
                // Use PostgreSQL test database when available
                services.AddDbContext<CareerDbContext>(options =>
                {
                    options.UseNpgsql(_databaseFixture.ConnectionString);
                });

                // Ensure the database is created
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<CareerDbContext>();
                db.Database.EnsureCreated();
            }
            else
            {
                // Use in-memory database as fallback when PostgreSQL is not available
                services.AddDbContext<CareerDbContext>(options =>
                {
                    options.UseInMemoryDatabase("CareerServiceTestDb");
                });
            }
        });

        builder.UseEnvironment("Testing");
    }

    public async Task InitializeAsync()
    {
        await _databaseFixture.InitializeAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _databaseFixture.DisposeAsync();
    }

    /// <summary>
    /// Creates a new DbContext for testing
    /// </summary>
    public CareerDbContext CreateDbContext()
    {
        return _databaseFixture.CreateDbContext();
    }

    /// <summary>
    /// Cleans all data from the test database
    /// </summary>
    public async Task CleanDatabaseAsync()
    {
        await _databaseFixture.CleanDatabaseAsync();
    }

    /// <summary>
    /// Clears the in-memory cache to prevent test pollution
    /// </summary>
    public void ClearCache()
    {
        using var scope = Services.CreateScope();
        var cache = scope.ServiceProvider.GetService<IMemoryCache>();
        if (cache is MemoryCache memoryCache)
        {
            // Compact 100% removes all cached entries
            memoryCache.Compact(1.0);
        }
    }
}
