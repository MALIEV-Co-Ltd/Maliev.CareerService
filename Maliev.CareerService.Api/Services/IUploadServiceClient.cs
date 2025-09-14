namespace Maliev.CareerService.Api.Services;

public interface IUploadServiceClient
{
    Task<UploadServiceResponse> UploadFileAsync(Stream fileStream, UploadServiceRequest request, CancellationToken cancellationToken = default);
    Task<string?> GetDownloadUrlAsync(string bucket, string objectName, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    Task<bool> DeleteFileAsync(string bucket, string objectName, CancellationToken cancellationToken = default);
}

public class UploadServiceRequest
{
    public required string FileName { get; set; }
    public required string ContentType { get; set; }
    public required string UploadPath { get; set; }
    public long FileSize { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
}

public class UploadServiceResponse
{
    public required string Bucket { get; set; }
    public required string ObjectName { get; set; }
    public required string Uri { get; set; }
    public long Size { get; set; }
}