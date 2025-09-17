using Maliev.CareerService.Api.Models;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace Maliev.CareerService.Api.HealthChecks;

public class UploadServiceHealthCheck : IHealthCheck
{
    private readonly HttpClient _httpClient;
    private readonly UploadServiceOptions _options;
    private readonly ILogger<UploadServiceHealthCheck> _logger;

    public UploadServiceHealthCheck(
        HttpClient httpClient,
        IOptions<UploadServiceOptions> options,
        ILogger<UploadServiceHealthCheck> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // Configure HttpClient if not already configured
            if (_httpClient.BaseAddress == null)
            {
                _httpClient.BaseAddress = new Uri(_options.BaseUrl);
                _httpClient.Timeout = TimeSpan.FromSeconds(Math.Min(_options.TimeoutSeconds, 10)); // Limit health check timeout
            }

            // Try to reach the upload service liveness endpoint
            var response = await _httpClient.GetAsync("/uploads/liveness", cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                return HealthCheckResult.Healthy($"UploadService is reachable at {_options.BaseUrl}");
            }
            else
            {
                return HealthCheckResult.Degraded($"UploadService responded with status code: {response.StatusCode}");
            }
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogWarning("UploadService health check timed out");
            return HealthCheckResult.Degraded("UploadService health check timed out");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "UploadService health check failed with HTTP error");
            return HealthCheckResult.Unhealthy($"UploadService is not reachable: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UploadService health check failed unexpectedly");
            return HealthCheckResult.Unhealthy($"UploadService health check failed: {ex.Message}");
        }
    }
}