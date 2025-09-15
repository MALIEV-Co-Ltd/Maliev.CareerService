using Google.Cloud.Storage.V1;
using Maliev.CareerService.Api.Models;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace Maliev.CareerService.Api.HealthChecks;

public class GcsHealthCheck : IHealthCheck
{
    private readonly GcsConfiguration _gcsConfig;
    private readonly ILogger<GcsHealthCheck> _logger;

    public GcsHealthCheck(
        IOptions<GcsConfiguration> gcsConfig,
        ILogger<GcsHealthCheck> logger)
    {
        _gcsConfig = gcsConfig.Value;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // Skip GCS health check if configuration is not provided (e.g., in development)
            if (string.IsNullOrEmpty(_gcsConfig.BasePath))
            {
                return HealthCheckResult.Healthy("GCS configuration not provided, skipping health check");
            }

            // Try to create a GCS client - this will validate credentials
            var storage = await StorageClient.CreateAsync();
            
            // Simple validation - just ensure we can create the client
            return HealthCheckResult.Healthy($"GCS client created successfully with base path {_gcsConfig.BasePath}");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "GCS health check failed");
            return HealthCheckResult.Unhealthy($"GCS health check failed: {ex.Message}", ex);
        }
    }
}