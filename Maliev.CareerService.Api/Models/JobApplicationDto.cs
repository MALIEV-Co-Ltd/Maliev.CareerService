namespace Maliev.CareerService.Api.Models;
/// <summary>
/// Data transfer object for JobApplication
/// </summary>

public class JobApplicationDto
{
    /// <summary>
    /// Gets or sets the job application identifier.
    /// </summary>
    public int Id { get; set; }
    /// <summary>
    /// Gets or sets the job position identifier.
    /// </summary>
    public int JobPositionId { get; set; }
    /// <summary>
    /// Gets or sets the applicant's email address.
    /// </summary>
    public required string ApplicantEmail { get; set; }
    /// <summary>
    /// Gets or sets the applicant's full name.
    /// </summary>
    public required string ApplicantName { get; set; }
    /// <summary>
    /// Gets or sets the applicant's phone number.
    /// </summary>
    public string? ApplicantPhone { get; set; }
    /// <summary>
    /// Gets or sets the applicant's LinkedIn profile URL.
    /// </summary>
    public string? LinkedInProfile { get; set; }
    /// <summary>
    /// Gets or sets the applicant's portfolio URL.
    /// </summary>
    public string? PortfolioUrl { get; set; }
    /// <summary>
    /// Gets or sets the current application status.
    /// </summary>
    public required string Status { get; set; }
    /// <summary>
    /// Gets or sets the date when the application was submitted.
    /// </summary>
    public DateTime ApplicationDate { get; set; }
    /// <summary>
    /// Gets or sets the date of the last status change.
    /// </summary>
    public DateTime LastStatusChange { get; set; }
    /// <summary>
    /// Gets or sets internal notes about the application.
    /// </summary>
    public string? Notes { get; set; }
    /// <summary>
    /// Gets or sets the creation date of the record.
    /// </summary>
    public DateTime CreatedDate { get; set; }
    /// <summary>
    /// Gets or sets the last modification date of the record.
    /// </summary>
    public DateTime ModifiedDate { get; set; }

    /// <summary>
    /// Gets or sets the associated job position details.
    /// </summary>
    public JobPositionDto? JobPosition { get; set; }
    /// <summary>
    /// Gets or sets the list of documents attached to the application.
    /// </summary>
    public List<ApplicationDocumentDto> Documents { get; set; } = [];
}
