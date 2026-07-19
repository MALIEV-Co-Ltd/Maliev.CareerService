using Maliev.CareerService.Application.Models.JobPostings;

namespace Maliev.CareerService.Application.Models.Applications;

/// <summary>
/// Response DTO for job application
/// </summary>
public class JobApplicationResponse
{
    /// <summary>
    /// Unique identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Job posting ID
    /// </summary>
    public Guid JobPostingId { get; set; }

    /// <summary>
    /// Job posting details
    /// </summary>
    public JobPostingResponse? JobPosting { get; set; }

    /// <summary>
    /// Applicant first name
    /// </summary>
    public string ApplicantFirstName { get; set; } = string.Empty;

    /// <summary>
    /// Applicant last name
    /// </summary>
    public string ApplicantLastName { get; set; } = string.Empty;

    /// <summary>
    /// Applicant full name
    /// </summary>
    public string ApplicantFullName => $"{ApplicantFirstName} {ApplicantLastName}";

    /// <summary>
    /// Applicant email
    /// </summary>
    public string ApplicantEmail { get; set; } = string.Empty;

    /// <summary>
    /// Applicant phone number
    /// </summary>
    public string? ApplicantPhone { get; set; }

    /// <summary>
    /// Applicant country code
    /// </summary>
    public string? ApplicantCountryCode { get; set; }

    /// <summary>
    /// Applicant country name (from Country Service)
    /// </summary>
    public string? ApplicantCountryName { get; set; }

    /// <summary>
    /// Resume file ID
    /// </summary>
    public Guid ResumeFileId { get; set; }

    /// <summary>
    /// Resume file URL (from Upload Service)
    /// </summary>
    public string? ResumeFileUrl { get; set; }

    /// <summary>
    /// Cover letter
    /// </summary>
    public string? CoverLetter { get; set; }

    /// <summary>
    /// Additional file IDs
    /// </summary>
    public Guid[] AdditionalFileIds { get; set; } = [];

    /// <summary>
    /// Additional file URLs (from Upload Service)
    /// </summary>
    public string[] AdditionalFileUrls { get; set; } = [];

    /// <summary>
    /// Application status
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// When the application was submitted
    /// </summary>
    public DateTime AppliedAt { get; set; }

    /// <summary>
    /// When the record was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When the record was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Optimistic concurrency token (Base64 encoded)
    /// </summary>
    public string RowVersion { get; set; } = string.Empty;
}
