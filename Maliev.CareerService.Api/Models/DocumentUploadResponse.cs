namespace Maliev.CareerService.Api.Models;

public class DocumentUploadResponse
{
    public int DocumentId { get; set; }
    public required string UploadUrl { get; set; }
    public required string DocumentType { get; set; }
    public required string OriginalFileName { get; set; }
    public DateTime UploadUrlExpiration { get; set; }
}