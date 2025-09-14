using System.ComponentModel.DataAnnotations;

namespace Maliev.CareerService.Api.Models;

public class CreateJobApplicationRequest
{
    [Required]
    public int JobPositionId { get; set; }

    [Required]
    [MaxLength(255)]
    [EmailAddress]
    public required string ApplicantEmail { get; set; }

    [Required]
    [MaxLength(200)]
    public required string ApplicantName { get; set; }

    [MaxLength(50)]
    [Phone]
    public string? ApplicantPhone { get; set; }

    [MaxLength(500)]
    [Url]
    public string? LinkedInProfile { get; set; }

    [MaxLength(500)]
    [Url]
    public string? PortfolioUrl { get; set; }

    [MaxLength(500)]
    public string? CoverLetterText { get; set; }

    public List<CreateDocumentMetadataRequest> DocumentMetadata { get; set; } = new();
}

public class CreateDocumentMetadataRequest
{
    [Required]
    [MaxLength(50)]
    public required string DocumentType { get; set; }

    [Required]
    [MaxLength(255)]
    public required string OriginalFileName { get; set; }

    [Required]
    public long FileSize { get; set; }

    [Required]
    [MaxLength(100)]
    public required string MimeType { get; set; }

    public bool IsRequired { get; set; } = false;

    public int DisplayOrder { get; set; } = 0;

    [MaxLength(500)]
    public string? Description { get; set; }
}