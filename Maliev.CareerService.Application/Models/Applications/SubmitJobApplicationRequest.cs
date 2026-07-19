using System.ComponentModel.DataAnnotations;

namespace Maliev.CareerService.Application.Models.Applications;

/// <summary>
/// Request to submit a job application
/// </summary>
public class SubmitJobApplicationRequest
{
    /// <summary>
    /// Job posting ID
    /// </summary>
    [Required(ErrorMessage = "Job posting ID is required")]
    public Guid JobPostingId { get; set; }

    /// <summary>
    /// Applicant first name
    /// </summary>
    [Required(ErrorMessage = "First name is required")]
    [StringLength(100, ErrorMessage = "First name cannot exceed 100 characters")]
    public string ApplicantFirstName { get; set; } = string.Empty;

    /// <summary>
    /// Applicant last name
    /// </summary>
    [Required(ErrorMessage = "Last name is required")]
    [StringLength(100, ErrorMessage = "Last name cannot exceed 100 characters")]
    public string ApplicantLastName { get; set; } = string.Empty;

    /// <summary>
    /// Applicant email
    /// </summary>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
    public string ApplicantEmail { get; set; } = string.Empty;

    /// <summary>
    /// Applicant phone number
    /// </summary>
    [Phone(ErrorMessage = "Invalid phone format")]
    [StringLength(20, ErrorMessage = "Phone cannot exceed 20 characters")]
    public string? ApplicantPhone { get; set; }

    /// <summary>
    /// Applicant country code (ISO 3166-1 alpha-2)
    /// </summary>
    [StringLength(2, MinimumLength = 2, ErrorMessage = "Country code must be exactly 2 characters")]
    [RegularExpression(@"^[A-Z]{2}$", ErrorMessage = "Country code must be 2 uppercase letters")]
    public string? ApplicantCountryCode { get; set; }

    /// <summary>
    /// Resume file ID from Upload Service
    /// </summary>
    [Required(ErrorMessage = "Resume file is required")]
    public Guid ResumeFileId { get; set; }

    /// <summary>
    /// Cover letter text
    /// </summary>
    [StringLength(5000, ErrorMessage = "Cover letter cannot exceed 5000 characters")]
    public string? CoverLetter { get; set; }

    /// <summary>
    /// Additional file IDs from Upload Service (max 4)
    /// </summary>
    [MaxLength(4, ErrorMessage = "Maximum 4 additional files allowed")]
    public Guid[] AdditionalFileIds { get; set; } = [];
}
