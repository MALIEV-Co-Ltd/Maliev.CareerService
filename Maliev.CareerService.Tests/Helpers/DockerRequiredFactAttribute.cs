using Maliev.CareerService.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Xunit;

namespace Maliev.CareerService.Tests.Helpers;

/// <summary>
/// Custom Fact attribute that skips tests when PostgreSQL is not available.
/// Checks appsettings.Testing.json and tests actual connection.
/// </summary>
public sealed class DockerRequiredFactAttribute : FactAttribute
{
    private static readonly Lazy<(bool available, string? skipReason)> _postgresStatus = new(CheckPostgreSqlAvailability);

    public DockerRequiredFactAttribute()
    {
        if (!_postgresStatus.Value.available)
        {
            Skip = _postgresStatus.Value.skipReason;
        }
    }

    private static (bool available, string? skipReason) CheckPostgreSqlAvailability()
    {
        try
        {
            // Load configuration from appsettings.Testing.json
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.Testing.json", optional: true)
                .Build();

            var connectionString = configuration.GetConnectionString("CareerDbContext");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return (false, "PostgreSQL not configured. Connection string 'CareerDbContext' not found in appsettings.Testing.json. Integration tests will be skipped.");
            }

            // Test actual connection
            var optionsBuilder = new DbContextOptionsBuilder<CareerDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            using var context = new CareerDbContext(optionsBuilder.Options);
            context.Database.CanConnect();

            return (true, null);
        }
        catch (Exception ex)
        {
            var message = ex is NpgsqlException || ex is TimeoutException
                ? $"PostgreSQL not available: {ex.Message}. Integration tests will be skipped. Start PostgreSQL or check connection settings in appsettings.Testing.json."
                : $"Failed to check PostgreSQL availability: {ex.Message}. Integration tests will be skipped.";

            return (false, message);
        }
    }
}
