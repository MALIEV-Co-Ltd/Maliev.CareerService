using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Maliev.CareerService.Api.HealthChecks;

public class ResponseTimeHealthCheck : IHealthCheck
{
    private const double WarningThresholdMs = 1000; // 1 second
    private const double CriticalThresholdMs = 3000; // 3 seconds
    
    // Static field to store response times - in a real implementation, this would use a proper metrics system
    public static readonly List<double> ResponseTimes = new List<double>();
    private static readonly object LockObject = new object();

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            lock (LockObject)
            {
                if (ResponseTimes.Count == 0)
                {
                    return Task.FromResult(HealthCheckResult.Healthy("No response times recorded yet"));
                }

                var averageResponseTime = ResponseTimes.Average();
                var p95ResponseTime = ResponseTimes.OrderBy(x => x).ElementAt((int)(ResponseTimes.Count * 0.95));
                var p99ResponseTime = ResponseTimes.OrderBy(x => x).ElementAt((int)(ResponseTimes.Count * 0.99));

                var data = new Dictionary<string, object>
                {
                    ["averageResponseTimeMs"] = Math.Round(averageResponseTime, 2),
                    ["p95ResponseTimeMs"] = Math.Round(p95ResponseTime, 2),
                    ["p99ResponseTimeMs"] = Math.Round(p99ResponseTime, 2),
                    ["totalRequests"] = ResponseTimes.Count
                };

                if (averageResponseTime > CriticalThresholdMs)
                {
                    return Task.FromResult(HealthCheckResult.Unhealthy(
                        $"Average response time is critically slow: {Math.Round(averageResponseTime, 2)} ms", 
                        null, 
                        data));
                }
                else if (averageResponseTime > WarningThresholdMs)
                {
                    return Task.FromResult(HealthCheckResult.Degraded(
                        $"Average response time is slower than expected: {Math.Round(averageResponseTime, 2)} ms", 
                        null, 
                        data));
                }
                else
                {
                    return Task.FromResult(HealthCheckResult.Healthy(
                        $"Response time is healthy: {Math.Round(averageResponseTime, 2)} ms average", 
                        data));
                }
            }
        }
        catch (Exception ex)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy($"Response time health check failed: {ex.Message}", ex));
        }
    }

    public static void RecordResponseTime(double responseTimeMs)
    {
        lock (LockObject)
        {
            ResponseTimes.Add(responseTimeMs);
            
            // Keep only the last 1000 response times to prevent memory issues
            if (ResponseTimes.Count > 1000)
            {
                ResponseTimes.RemoveRange(0, ResponseTimes.Count - 1000);
            }
        }
    }
}