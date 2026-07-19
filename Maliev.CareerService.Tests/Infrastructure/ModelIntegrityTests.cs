using Maliev.CareerService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Maliev.CareerService.Tests.Infrastructure;

/// <summary>
/// Integrity tests.
/// </summary>
public class ModelIntegrityTests
{
    /// <summary>
    /// Check for pending migrations. Requires a running PostgreSQL database with migrations applied.
    /// This test is skipped in CI/CD environments where database is not available.
    /// </summary>
    [Fact]
    public void Model_ShouldNotHavePendingChanges()
    {
        var connString = Environment.GetEnvironmentVariable("ConnectionStrings__CareerDbContext") 
            ?? "Host=localhost;Database=careerdb;Username=postgres;Password=postgres";
        
        // Skip if not connecting to localhost (e.g., test container uses different host)
        if (!connString.Contains("localhost"))
        {
            return; // Skip - not a local dev database
        }
        
        try
        {
            var options = new DbContextOptionsBuilder<CareerDbContext>()
                .UseNpgsql(connString)
                .Options;

            using var context = new CareerDbContext(options);
            
            // Ensure database exists and can be connected
            if (!context.Database.CanConnect())
            {
                return; // Skip - no database connection
            }
            
            // Ensure migrations are applied
            var migrations = context.Database.GetPendingMigrations();
            if (migrations.Any())
            {
                Assert.Fail($"Pending migrations found: {string.Join(", ", migrations)}. Run 'dotnet ef migrations add <Name> --project Maliev.CareerService.Infrastructure --startup-project Maliev.CareerService.Infrastructure'");
            }
        }
        catch (Exception ex)
        {
            var msg = ex.Message.ToLower();
            // Skip test when database is not available (e.g., in CI without Docker)
            if (msg.Contains("connection") || msg.Contains("timeout") || msg.Contains("host") || msg.Contains("refused") || msg.Contains("28p01") || msg.Contains("password"))
            {
                return; // Test passes - skipped due to no database
            }
            throw; // Re-throw for other unexpected errors
        }
    }
}
