using Maliev.CareerService.Data.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Maliev.CareerService.Api.HealthChecks;

public class BusinessMetricsHealthCheck : IHealthCheck
{
    private readonly CareerDbContext _context;

    public BusinessMetricsHealthCheck(CareerDbContext context)
    {
        _context = context;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // Get business metrics
            var totalJobPositions = await _context.JobPositions.CountAsync(cancellationToken);
            var totalJobApplications = await _context.JobApplications.CountAsync(cancellationToken);
            var activeJobPositions = await _context.JobPositions.CountAsync(jp => jp.IsActive, cancellationToken);
            
            // Get recent activity (last 24 hours)
            var recentApplications = await _context.JobApplications
                .CountAsync(ja => ja.ApplicationDate >= DateTime.UtcNow.AddDays(-1), cancellationToken);
            
            var recentJobPositions = await _context.JobPositions
                .CountAsync(jp => jp.CreatedDate >= DateTime.UtcNow.AddDays(-1), cancellationToken);

            var data = new Dictionary<string, object>
            {
                ["totalJobPositions"] = totalJobPositions,
                ["totalJobApplications"] = totalJobApplications,
                ["activeJobPositions"] = activeJobPositions,
                ["recentApplications24h"] = recentApplications,
                ["recentJobPositions24h"] = recentJobPositions
            };

            // Check for healthy business metrics
            if (totalJobPositions == 0)
            {
                return HealthCheckResult.Degraded("No job positions found in the system", null, data);
            }
            
            if (activeJobPositions == 0)
            {
                return HealthCheckResult.Degraded("No active job positions available", null, data);
            }

            return HealthCheckResult.Healthy("Business metrics are healthy", data);
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"Business metrics health check failed: {ex.Message}", ex);
        }
    }
}