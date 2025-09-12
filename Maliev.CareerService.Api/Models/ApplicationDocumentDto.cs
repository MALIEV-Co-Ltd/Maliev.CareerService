namespace Maliev.CareerService.Api.Models;

public class ApplicationDocumentDto
{
    public int Id { get; set; }
    public int JobApplicationId { get; set; }
    public required string DocumentType { get; set; }
    public required string OriginalFileName { get; set; }
    public required string GcsBucket { get; set; }
    public required string GcsObjectName { get; set; }
    public required string GcsUri { get; set; }
    public long FileSize { get; set; }
    public required string MimeType { get; set; }
    public DateTime UploadDate { get; set; }
    public bool IsRequired { get; set; }
    public int DisplayOrder { get; set; }
    public string? Description { get; set; }
}