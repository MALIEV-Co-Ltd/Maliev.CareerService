using System.Net.Http.Json;
using Microsoft.Extensions.Options;

namespace Maliev.CareerService.Api.Services.External;

/// <summary>
/// HTTP client implementation for registering the service with central IAM.
/// </summary>
public class IamServiceClient : IIamServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<IamServiceClient> _logger;

    /// <summary>Initializes a new instance of the IAM client.</summary>
    public IamServiceClient(
        HttpClient httpClient,
        IOptions<IamServiceOptions> options,
        ILogger<IamServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _httpClient.BaseAddress = new Uri(options.Value.BaseUrl);
    }

    /// <inheritdoc/>
    public async Task RegisterManifestAsync(IamManifest manifest, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Registering IAM manifest for {ServiceName}", manifest.ServiceName);

            var response = await _httpClient.PostAsJsonAsync("/iam/v1/register", manifest, cancellationToken);

            response.EnsureSuccessStatusCode();

            _logger.LogInformation("Successfully registered IAM manifest for {ServiceName}", manifest.ServiceName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to register IAM manifest for {ServiceName}", manifest.ServiceName);
            // We don't throw here to allow the service to start even if IAM registration fails temporarily
        }
    }
}

/// <summary>Configuration options for the IAM service integration.</summary>
public class IamServiceOptions
{
    /// <summary>Gets or sets the base URL of the IAM service.</summary>
    public string BaseUrl { get; set; } = string.Empty;
}