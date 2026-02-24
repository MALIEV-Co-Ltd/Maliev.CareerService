using Maliev.CareerService.Data.Models.Base;

namespace Maliev.CareerService.Data.Models;

/// <summary>
/// Job application submitted by external applicant
/// </summary>
public class JobApplication : BaseEntity
{
    /// <summary>
    /// Foreign key to JobPosting
    /// </summary>
    public Guid JobPostingId { get; set; }

    /// <summary>
    /// Navigation property to JobPosting
    /// </summary>
    public JobPosting JobPosting { get; set; } = null!;

    /// <summary>
    /// Applicant first name
    /// </summary>
    public string ApplicantFirstName { get; set; } = string.Empty;

    /// <summary>
    /// Applicant last name
    /// </summary>
    public string ApplicantLastName { get; set; } = string.Empty;

    /// <summary>
    /// Applicant email address
    /// </summary>
    public string ApplicantEmail { get; set; } = string.Empty;

    /// <summary>
    /// Applicant phone number
    /// </summary>
    public string? ApplicantPhone { get; set; }

    /// <summary>
    /// Applicant country code (ISO 3166-1 alpha-2)
    /// </summary>
    public string? ApplicantCountryCode { get; set; }

    /// <summary>
    /// Resume file ID from Upload Service
    /// </summary>
    public Guid ResumeFileId { get; set; }

    /// <summary>
    /// Cover letter text
    /// </summary>
    public string? CoverLetter { get; set; }

    /// <summary>
    /// Additional file IDs from Upload Service (max 4 additional files)
    /// </summary>
    public Guid[] AdditionalFileIds { get; set; } = [];

    /// <summary>
    /// Application status (Submitted, UnderReview, Interviewing, Offered, Accepted, Rejected, Withdrawn)
    /// </summary>
    public string Status { get; set; } = ApplicationStatus.Submitted;

    /// <summary>
    /// When the application was submitted
    /// </summary>
    public DateTime AppliedAt { get; set; }

    /// <summary>
    /// Status change history
    /// </summary>
    public ICollection<ApplicationStatusChange> StatusChanges { get; set; } = [];
}
