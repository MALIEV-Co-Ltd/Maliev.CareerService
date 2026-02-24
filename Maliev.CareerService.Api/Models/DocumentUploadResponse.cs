namespace Maliev.CareerService.Api.Models;
/// <summary>
/// Response model for documentupload
/// </summary>

public class DocumentUploadResponse
{
    /// <summary>
    /// Gets or sets the document identifier.
    /// </summary>
    public int DocumentId { get; set; }
    /// <summary>
    /// Gets or sets the temporary upload URL for the document.
    /// </summary>
    public required string UploadUrl { get; set; }
    /// <summary>
    /// Gets or sets the type of document.
    /// </summary>
    public required string DocumentType { get; set; }
    /// <summary>
    /// Gets or sets the original file name.
    /// </summary>
    public required string OriginalFileName { get; set; }
    /// <summary>
    /// Gets or sets the expiration time for the upload URL.
    /// </summary>
    public DateTime UploadUrlExpiration { get; set; }
}
