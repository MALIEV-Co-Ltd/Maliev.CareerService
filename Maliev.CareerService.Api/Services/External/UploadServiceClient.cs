using System.Net.Http.Json;
using Microsoft.Extensions.Options;


namespace Maliev.CareerService.Api.Services.External;

/// <summary>
/// HTTP client implementation for Upload Service integration
/// </summary>
public class UploadServiceClient : IUploadServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UploadServiceClient> _logger;


    public UploadServiceClient(
        HttpClient httpClient,
        IOptions<UploadServiceOptions> options,
        ILogger<UploadServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        // Configure base URL from options
        _httpClient.BaseAddress = new Uri(options.Value.BaseUrl);
    }

    /// <inheritdoc />
    public async Task<bool> ValidateFileAsync(Guid fileId, CancellationToken cancellationToken = default)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Head, $"/files/v1/{fileId}");
            var response = await _httpClient.SendAsync(request, cancellationToken);

            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to validate file {FileId} from Upload Service", fileId);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<string?> GetFileUrlAsync(Guid fileId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/files/v1/{fileId}/url", cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();

            var fileUrlResponse = await response.Content.ReadFromJsonAsync<FileUrlResponse>(cancellationToken);
            return fileUrlResponse?.Url;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to get file URL for {FileId} from Upload Service", fileId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<Dictionary<Guid, string>> GetFileUrlsAsync(IEnumerable<Guid> fileIds, CancellationToken cancellationToken = default)
    {
        var result = new Dictionary<Guid, string>();
        var fileIdsList = fileIds.ToList();

        if (fileIdsList.Count == 0)
        {
            return result;
        }

        try
        {
            // Use batch endpoint if available, otherwise fetch individually
            var tasks = fileIdsList.Select(async fileId =>
            {
                var url = await GetFileUrlAsync(fileId, cancellationToken);
                return new { FileId = fileId, Url = url };
            });

            var results = await Task.WhenAll(tasks);

            foreach (var item in results.Where(x => x.Url != null))
            {
                result[item.FileId] = item.Url!;
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get file URLs from Upload Service");
            throw;
        }
    }
}

/// <summary>
/// Configuration options for Upload Service
/// </summary>
public class UploadServiceOptions
{
    public string BaseUrl { get; set; } = string.Empty;
}

/// <summary>
/// File URL response from Upload Service
/// </summary>
internal record FileUrlResponse(string Url);
