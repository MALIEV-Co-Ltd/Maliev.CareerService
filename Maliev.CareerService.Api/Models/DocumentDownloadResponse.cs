namespace Maliev.CareerService.Api.Models;

public class DocumentDownloadResponse
{
    public required string DownloadUrl { get; set; }
    public required string OriginalFileName { get; set; }
    public required string MimeType { get; set; }
    public long FileSize { get; set; }
    public DateTime DownloadUrlExpiration { get; set; }
}