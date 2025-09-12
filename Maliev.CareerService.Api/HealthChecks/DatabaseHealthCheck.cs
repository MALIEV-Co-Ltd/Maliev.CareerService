using Maliev.CareerService.Data.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Maliev.CareerService.Api.HealthChecks;

public class DatabaseHealthCheck : IHealthCheck
{
    private readonly CareerDbContext _context;

    public DatabaseHealthCheck(CareerDbContext context)
    {
        _context = context;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // Attempt to connect to the database and execute a simple query
            await _context.Database.ExecuteSqlRawAsync("SELECT 1", cancellationToken);
            
            return HealthCheckResult.Healthy("Database connection is healthy");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Database connection failed", ex);
        }
    }
}