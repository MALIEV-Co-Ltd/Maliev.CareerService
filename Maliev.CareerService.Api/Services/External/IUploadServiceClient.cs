namespace Maliev.CareerService.Api.Services.External;

/// <summary>
/// Client for Upload Service integration
/// </summary>
public interface IUploadServiceClient
{
    /// <summary>
    /// Validates if a file ID exists and is accessible
    /// </summary>
    Task<bool> ValidateFileAsync(Guid fileId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the URL for accessing a file
    /// </summary>
    Task<string?> GetFileUrlAsync(Guid fileId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets URLs for multiple files
    /// </summary>
    Task<Dictionary<Guid, string>> GetFileUrlsAsync(IEnumerable<Guid> fileIds, CancellationToken cancellationToken = default);
}
