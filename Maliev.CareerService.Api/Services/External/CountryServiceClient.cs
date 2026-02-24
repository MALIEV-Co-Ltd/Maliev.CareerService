namespace Maliev.CareerService.Api.Services.External;

/// <summary>
/// HTTP client implementation for Country Service integration
/// </summary>
public class CountryServiceClient : ICountryServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CountryServiceClient> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CountryServiceClient"/> class.
    /// </summary>

    public CountryServiceClient(
        HttpClient httpClient,
        ILogger<CountryServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<string?> GetCountryNameAsync(string countryCode, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/country/v1/{countryCode.ToUpperInvariant()}", cancellationToken);

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
/// Country response from Country Service
/// </summary>
internal record CountryResponse(string Code, string Name);
