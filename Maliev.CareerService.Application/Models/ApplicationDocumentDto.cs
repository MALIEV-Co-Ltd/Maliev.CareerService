namespace Maliev.CareerService.Application.Models;
/// <summary>
/// Data transfer object for ApplicationDocument
/// </summary>

public class ApplicationDocumentDto
{
    /// <summary>
    /// Gets or sets the document identifier.
    /// </summary>
    public int Id { get; set; }
    /// <summary>
    /// Gets or sets the job application identifier.
    /// </summary>
    public int JobApplicationId { get; set; }
    /// <summary>
    /// Gets or sets the document type.
    /// </summary>
    public required string DocumentType { get; set; }
    /// <summary>
    /// Gets or sets the original file name.
    /// </summary>
    public required string OriginalFileName { get; set; }
    /// <summary>
    /// Gets or sets the Google Cloud Storage bucket name.
    /// </summary>
    public required string GcsBucket { get; set; }
    /// <summary>
    /// Gets or sets the GCS object name.
    /// </summary>
    public required string GcsObjectName { get; set; }
    /// <summary>
    /// Gets or sets the full GCS URI.
    /// </summary>
    public required string GcsUri { get; set; }
    /// <summary>
    /// Gets or sets the file size in bytes.
    /// </summary>
    public long FileSize { get; set; }
    /// <summary>
    /// Gets or sets the MIME type.
    /// </summary>
    public required string MimeType { get; set; }
    /// <summary>
    /// Gets or sets the upload date.
    /// </summary>
    public DateTime UploadDate { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether this document is required.
    /// </summary>
    public bool IsRequired { get; set; }
    /// <summary>
    /// Gets or sets the display order.
    /// </summary>
    public int DisplayOrder { get; set; }
    /// <summary>
    /// Gets or sets the document description.
    /// </summary>
    public string? Description { get; set; }
}
