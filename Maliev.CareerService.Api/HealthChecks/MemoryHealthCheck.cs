using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Diagnostics;

namespace Maliev.CareerService.Api.HealthChecks;

public class MemoryHealthCheck : IHealthCheck
{
    private const long WarningThresholdBytes = 800L * 1024 * 1024; // 800MB
    private const long CriticalThresholdBytes = 1024L * 1024 * 1024; // 1GB

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var process = Process.GetCurrentProcess();
            var workingSet = process.WorkingSet64;
            var managedMemory = GC.GetTotalMemory(false);
            
            var data = new Dictionary<string, object>
            {
                ["workingSetBytes"] = workingSet,
                ["managedMemoryBytes"] = managedMemory,
                ["workingSetMB"] = workingSet / (1024 * 1024),
                ["managedMemoryMB"] = managedMemory / (1024 * 1024)
            };

            if (workingSet > CriticalThresholdBytes)
            {
                return Task.FromResult(HealthCheckResult.Unhealthy(
                    $"Memory usage is critical: {workingSet / (1024 * 1024)} MB", 
                    null, 
                    data));
            }
            else if (workingSet > WarningThresholdBytes)
            {
                return Task.FromResult(HealthCheckResult.Degraded(
                    $"Memory usage is high: {workingSet / (1024 * 1024)} MB", 
                    null, 
                    data));
            }
            else
            {
                return Task.FromResult(HealthCheckResult.Healthy(
                    $"Memory usage is healthy: {workingSet / (1024 * 1024)} MB", 
                    data));
            }
        }
        catch (Exception ex)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy($"Memory health check failed: {ex.Message}", ex));
        }
    }
}