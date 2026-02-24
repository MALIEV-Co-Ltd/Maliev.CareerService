using Maliev.CareerService.Api.Services.External;

namespace Maliev.CareerService.Tests.Mocks;

/// <summary>
/// Mock implementation of ICountryServiceClient for testing
/// </summary>
public class MockCountryServiceClient : ICountryServiceClient
{
    private readonly Dictionary<string, string> _countries = new()
    {
        { "US", "United States" },
        { "GB", "United Kingdom" },
        { "CA", "Canada" },
        { "TH", "Thailand" },
        { "JP", "Japan" },
        { "SG", "Singapore" },
        { "AU", "Australia" },
        { "DE", "Germany" },
        { "FR", "France" }
    };

    public Task<string?> GetCountryNameAsync(string countryCode, CancellationToken cancellationToken = default)
    {
        _countries.TryGetValue(countryCode.ToUpperInvariant(), out var countryName);
        return Task.FromResult(countryName);
    }

    public Task<Dictionary<string, string>> GetCountryNamesAsync(IEnumerable<string> countryCodes, CancellationToken cancellationToken = default)
    {
        var result = new Dictionary<string, string>();
        foreach (var code in countryCodes)
        {
            if (_countries.TryGetValue(code.ToUpperInvariant(), out var name))
            {
                result[code] = name;
            }
        }
        return Task.FromResult(result);
    }

    public void AddCountry(string countryCode, string countryName)
    {
        _countries[countryCode.ToUpperInvariant()] = countryName;
    }
}
