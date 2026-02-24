using Maliev.CareerService.Data.Models.Base;

namespace Maliev.CareerService.Data.Models;

/// <summary>
/// Job posting entity for external applicants
/// </summary>
public class JobPosting : BaseEntity
{
    /// <summary>
    /// Position title
    /// </summary>
    public string PositionTitle { get; set; } = string.Empty;

    /// <summary>
    /// Unique position code
    /// </summary>
    public string PositionCode { get; set; } = string.Empty;

    /// <summary>
    /// Department
    /// </summary>
    public string? Department { get; set; }

    /// <summary>
    /// Work location
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// Employment type (Full-time, Part-time, Contract, Internship)
    /// </summary>
    public string EmploymentType { get; set; } = string.Empty;

    /// <summary>
    /// Minimum salary
    /// </summary>
    public decimal? SalaryMin { get; set; }

    /// <summary>
    /// Maximum salary
    /// </summary>
    public decimal? SalaryMax { get; set; }

    /// <summary>
    /// Currency code (USD, THB, etc.)
    /// </summary>
    public string? Currency { get; set; }

    /// <summary>
    /// Job description in Markdown format
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Job requirements in Markdown format
    /// </summary>
    public string Requirements { get; set; } = string.Empty;

    /// <summary>
    /// Job responsibilities in Markdown format
    /// </summary>
    public string Responsibilities { get; set; } = string.Empty;

    /// <summary>
    /// Application deadline
    /// </summary>
    public DateTime ApplicationDeadline { get; set; }

    /// <summary>
    /// When the posting was published
    /// </summary>
    public DateTime? PublishedAt { get; set; }

    /// <summary>
    /// Whether the posting is active and accepting applications
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Applications submitted for this posting
    /// </summary>
    public ICollection<JobApplication> Applications { get; set; } = [];
}
