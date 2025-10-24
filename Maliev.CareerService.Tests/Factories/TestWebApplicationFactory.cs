using Maliev.CareerService.Data;
using Maliev.CareerService.Tests.Fixtures;
using Maliev.CareerService.Tests.Helpers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
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

            // Skip database configuration if PostgreSQL isn't available
            if (!_databaseFixture.IsAvailable)
            {
                return;
            }

            // Remove the existing DbContext registration
            services.RemoveAll<DbContextOptions<CareerDbContext>>();
            services.RemoveAll<CareerDbContext>();

            // Add DbContext using PostgreSQL test database
            services.AddDbContext<CareerDbContext>(options =>
            {
                options.UseNpgsql(_databaseFixture.ConnectionString);
            });

            // Ensure the database is created
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CareerDbContext>();
            db.Database.EnsureCreated();
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
}
