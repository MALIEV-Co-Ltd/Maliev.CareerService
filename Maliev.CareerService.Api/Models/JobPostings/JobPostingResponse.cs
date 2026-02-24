namespace Maliev.CareerService.Api.Models.JobPostings;

/// <summary>
/// Response DTO for job posting
/// </summary>
public class JobPostingResponse
{
    /// <summary>
    /// Unique identifier
    /// </summary>
    public Guid Id { get; set; }

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
    /// Employment type
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
    /// Currency code
    /// </summary>
    public string? Currency { get; set; }

    /// <summary>
    /// Job description (Markdown)
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Job description rendered as HTML
    /// </summary>
    public string DescriptionHtml { get; set; } = string.Empty;

    /// <summary>
    /// Job requirements (Markdown)
    /// </summary>
    public string Requirements { get; set; } = string.Empty;

    /// <summary>
    /// Job requirements rendered as HTML
    /// </summary>
    public string RequirementsHtml { get; set; } = string.Empty;

    /// <summary>
    /// Job responsibilities (Markdown)
    /// </summary>
    public string Responsibilities { get; set; } = string.Empty;

    /// <summary>
    /// Job responsibilities rendered as HTML
    /// </summary>
    public string ResponsibilitiesHtml { get; set; } = string.Empty;

    /// <summary>
    /// Application deadline
    /// </summary>
    public DateTime ApplicationDeadline { get; set; }

    /// <summary>
    /// When the posting was published
    /// </summary>
    public DateTime? PublishedAt { get; set; }

    /// <summary>
    /// Whether the posting is active
    /// </summary>
    public bool IsActive { get; set; }

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
