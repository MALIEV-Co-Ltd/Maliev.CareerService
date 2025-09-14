using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Maliev.CareerService.Api.Services;

public class UploadServiceClient : IUploadServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UploadServiceClient> _logger;

    public UploadServiceClient(HttpClient httpClient, ILogger<UploadServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<UploadServiceResponse> UploadFileAsync(Stream fileStream, UploadServiceRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            using var content = new MultipartFormDataContent();
            
            // Add file stream
            var fileContent = new StreamContent(fileStream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(request.ContentType);
            content.Add(fileContent, "file", request.FileName);
            
            // Add metadata
            content.Add(new StringContent(request.FileName), "fileName");
            content.Add(new StringContent(request.ContentType), "contentType");
            content.Add(new StringContent(request.UploadPath), "uploadPath");
            content.Add(new StringContent(request.FileSize.ToString()), "fileSize");
            
            // Add custom metadata as JSON
            if (request.Metadata.Any())
            {
                var metadataJson = JsonSerializer.Serialize(request.Metadata);
                content.Add(new StringContent(metadataJson, Encoding.UTF8, "application/json"), "metadata");
            }

            var response = await _httpClient.PostAsync("/upload/custom-path", content, cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var uploadResponse = JsonSerializer.Deserialize<UploadServiceResponse>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (uploadResponse == null)
            {
                throw new InvalidOperationException("Failed to deserialize upload service response");
            }

            _logger.LogInformation("Successfully uploaded file {FileName} to {ObjectName}", request.FileName, uploadResponse.ObjectName);
            return uploadResponse;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error uploading file {FileName}", request.FileName);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file {FileName}", request.FileName);
            throw;
        }
    }

    public async Task<string?> GetDownloadUrlAsync(string bucket, string objectName, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var expirationParam = expiration?.TotalMinutes.ToString() ?? "60";
            var response = await _httpClient.GetAsync($"/upload/download-url?bucket={bucket}&objectName={Uri.EscapeDataString(objectName)}&expirationMinutes={expirationParam}", cancellationToken);
            
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();
            
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var urlResponse = JsonSerializer.Deserialize<DownloadUrlResponse>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return urlResponse?.DownloadUrl;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error getting download URL for {ObjectName}", objectName);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting download URL for {ObjectName}", objectName);
            return null;
        }
    }

    public async Task<bool> DeleteFileAsync(string bucket, string objectName, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"/upload/delete?bucket={bucket}&objectName={Uri.EscapeDataString(objectName)}", cancellationToken);
            
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("File {ObjectName} not found for deletion", objectName);
                return true; // Consider not found as successfully deleted
            }

            response.EnsureSuccessStatusCode();
            _logger.LogInformation("Successfully deleted file {ObjectName}", objectName);
            return true;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error deleting file {ObjectName}", objectName);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file {ObjectName}", objectName);
            return false;
        }
    }

    private class DownloadUrlResponse
    {
        public string? DownloadUrl { get; set; }
    }
}