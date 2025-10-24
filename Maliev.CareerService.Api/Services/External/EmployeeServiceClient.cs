using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;

namespace Maliev.CareerService.Api.Services.External;

/// <summary>
/// HTTP client implementation for Employee Service integration
/// </summary>
public class EmployeeServiceClient : IEmployeeServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<EmployeeServiceClient> _logger;
    private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;

    public EmployeeServiceClient(
        HttpClient httpClient,
        IOptions<EmployeeServiceOptions> options,
        ILogger<EmployeeServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        // Configure base URL from options
        _httpClient.BaseAddress = new Uri(options.Value.BaseUrl);

        // Configure Polly retry policy with exponential backoff (3 retries)
        _retryPolicy = Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .OrResult(r => !r.IsSuccessStatusCode && (int)r.StatusCode >= 500)
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                onRetry: (outcome, timespan, retryAttempt, context) =>
                {
                    _logger.LogWarning(
                        "Employee Service request failed with {StatusCode}. Retry attempt {RetryAttempt} after {Delay}s",
                        outcome.Result?.StatusCode,
                        retryAttempt,
                        timespan.TotalSeconds);
                });
    }

    /// <inheritdoc />
    public async Task<EmployeeResponse?> GetEmployeeAsync(Guid employeeId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _retryPolicy.ExecuteAsync(async () =>
                await _httpClient.GetAsync($"/employees/v1/{employeeId}", cancellationToken));

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<EmployeeResponse>(cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to get employee {EmployeeId} from Employee Service", employeeId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> ValidateEmployeeAsync(Guid employeeId, CancellationToken cancellationToken = default)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Head, $"/employees/v1/{employeeId}");
            var response = await _retryPolicy.ExecuteAsync(async () =>
                await _httpClient.SendAsync(request, cancellationToken));

            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to validate employee {EmployeeId} from Employee Service", employeeId);
            return false;
        }
    }
}

/// <summary>
/// Configuration options for Employee Service
/// </summary>
public class EmployeeServiceOptions
{
    public string BaseUrl { get; set; } = string.Empty;
}
