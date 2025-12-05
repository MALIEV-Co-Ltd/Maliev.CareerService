using System.ComponentModel.DataAnnotations;

namespace Maliev.CareerService.Api.Models;
/// <summary>
/// Request model for documentupload
/// </summary>

public class DocumentUploadRequest
{
    /// <summary>
    /// Gets or sets the file to upload.
    /// </summary>
    [Required]
    public required IFormFile File { get; set; }

    /// <summary>
    /// Gets or sets the type of document being uploaded.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public required string DocumentType { get; set; }

    /// <summary>
    /// Gets or sets an optional description for the document.
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this document is required.
    /// </summary>
    public bool IsRequired { get; set; } = false;

    /// <summary>
    /// Gets or sets the display order for this document.
    /// </summary>
    public int DisplayOrder { get; set; } = 0;
}
