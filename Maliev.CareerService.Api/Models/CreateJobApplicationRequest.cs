using System.ComponentModel.DataAnnotations;

namespace Maliev.CareerService.Api.Models;
/// <summary>
/// Request model for createjobapplication
/// </summary>

public class CreateJobApplicationRequest
{
    /// <summary>
    /// Gets or sets the job position identifier.
    /// </summary>
    [Required]
    public int JobPositionId { get; set; }

    /// <summary>
    /// Gets or sets the applicant's email address.
    /// </summary>
    [Required]
    [MaxLength(255)]
    [EmailAddress]
    public required string ApplicantEmail { get; set; }

    /// <summary>
    /// Gets or sets the applicant's full name.
    /// </summary>
    [Required]
    [MaxLength(200)]
    public required string ApplicantName { get; set; }

    /// <summary>
    /// Gets or sets the applicant's phone number.
    /// </summary>
    [MaxLength(50)]
    [Phone]
    public string? ApplicantPhone { get; set; }

    /// <summary>
    /// Gets or sets the applicant's LinkedIn profile URL.
    /// </summary>
    [MaxLength(500)]
    [Url]
    public string? LinkedInProfile { get; set; }

    /// <summary>
    /// Gets or sets the applicant's portfolio URL.
    /// </summary>
    [MaxLength(500)]
    [Url]
    public string? PortfolioUrl { get; set; }

    /// <summary>
    /// Gets or sets the cover letter text.
    /// </summary>
    [MaxLength(500)]
    public string? CoverLetterText { get; set; }

    /// <summary>
    /// Gets or sets the list of document metadata.
    /// </summary>
    public List<CreateDocumentMetadataRequest> DocumentMetadata { get; set; } = [];
}
/// <summary>
/// Request model for createdocumentmetadata
/// </summary>

public class CreateDocumentMetadataRequest
{
    /// <summary>
    /// Gets or sets the document type.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public required string DocumentType { get; set; }

    /// <summary>
    /// Gets or sets the original file name.
    /// </summary>
    [Required]
    [MaxLength(255)]
    public required string OriginalFileName { get; set; }

    /// <summary>
    /// Gets or sets the file size in bytes.
    /// </summary>
    [Required]
    public long FileSize { get; set; }

    /// <summary>
    /// Gets or sets the MIME type.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public required string MimeType { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this document is required.
    /// </summary>
    public bool IsRequired { get; set; } = false;

    /// <summary>
    /// Gets or sets the display order.
    /// </summary>
    public int DisplayOrder { get; set; } = 0;

    /// <summary>
    /// Gets or sets the document description.
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }
}
