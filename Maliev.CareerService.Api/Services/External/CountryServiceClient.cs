using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;

namespace Maliev.CareerService.Api.Services.External;

/// <summary>
/// HTTP client implementation for Country Service integration
/// </summary>
public class CountryServiceClient : ICountryServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CountryServiceClient> _logger;
    private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;

    public CountryServiceClient(
        HttpClient httpClient,
        IOptions<CountryServiceOptions> options,
        ILogger<CountryServiceClient> logger)
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
                        "Country Service request failed with {StatusCode}. Retry attempt {RetryAttempt} after {Delay}s",
                        outcome.Result?.StatusCode,
                        retryAttempt,
                        timespan.TotalSeconds);
                });
    }

    /// <inheritdoc />
    public async Task<string?> GetCountryNameAsync(string countryCode, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _retryPolicy.ExecuteAsync(async () =>
                await _httpClient.GetAsync($"/countries/v1/{countryCode.ToUpperInvariant()}", cancellationToken));

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();

            var countryResponse = await response.Content.ReadFromJsonAsync<CountryResponse>(cancellationToken);
            return countryResponse?.Name;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to get country name for {CountryCode} from Country Service", countryCode);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<Dictionary<string, string>> GetCountryNamesAsync(IEnumerable<string> countryCodes, CancellationToken cancellationToken = default)
    {
        var result = new Dictionary<string, string>();
        var countryCodesList = countryCodes.Distinct().ToList();

        if (countryCodesList.Count == 0)
        {
            return result;
        }

        try
        {
            // Fetch individually (could be optimized with batch endpoint if available)
            var tasks = countryCodesList.Select(async code =>
            {
                var name = await GetCountryNameAsync(code, cancellationToken);
                return new { Code = code, Name = name };
            });

            var results = await Task.WhenAll(tasks);

            foreach (var item in results.Where(x => x.Name != null))
            {
                result[item.Code] = item.Name!;
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get country names from Country Service");
            throw;
        }
    }
}

/// <summary>
/// Configuration options for Country Service
/// </summary>
public class CountryServiceOptions
{
    public string BaseUrl { get; set; } = string.Empty;
}

/// <summary>
/// Country response from Country Service
/// </summary>
internal record CountryResponse(string Code, string Name);
