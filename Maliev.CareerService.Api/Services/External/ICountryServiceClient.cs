namespace Maliev.CareerService.Api.Services.External;

/// <summary>
/// Client for Country Service integration
/// </summary>
public interface ICountryServiceClient
{
    /// <summary>
    /// Gets the country name by country code (ISO 3166-1 alpha-2)
    /// </summary>
    Task<string?> GetCountryNameAsync(string countryCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets country names for multiple country codes
    /// </summary>
    Task<Dictionary<string, string>> GetCountryNamesAsync(IEnumerable<string> countryCodes, CancellationToken cancellationToken = default);
}
