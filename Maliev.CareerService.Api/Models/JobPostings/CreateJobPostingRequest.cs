using System.ComponentModel.DataAnnotations;

namespace Maliev.CareerService.Api.Models.JobPostings;

/// <summary>
/// Request to create a new job posting
/// </summary>
public class CreateJobPostingRequest
{
    /// <summary>
    /// Position title
    /// </summary>
    [Required(ErrorMessage = "Position title is required")]
    [StringLength(200, ErrorMessage = "Position title cannot exceed 200 characters")]
    public string PositionTitle { get; set; } = string.Empty;

    /// <summary>
    /// Unique position code
    /// </summary>
    [Required(ErrorMessage = "Position code is required")]
    [StringLength(50, ErrorMessage = "Position code cannot exceed 50 characters")]
    [RegularExpression(@"^[A-Z0-9-]+$", ErrorMessage = "Position code must contain only uppercase letters, numbers, and hyphens")]
    public string PositionCode { get; set; } = string.Empty;

    /// <summary>
    /// Department
    /// </summary>
    [StringLength(100, ErrorMessage = "Department cannot exceed 100 characters")]
    public string? Department { get; set; }

    /// <summary>
    /// Work location
    /// </summary>
    [StringLength(100, ErrorMessage = "Location cannot exceed 100 characters")]
    public string? Location { get; set; }

    /// <summary>
    /// Employment type (Full-time, Part-time, Contract, Internship)
    /// </summary>
    [Required(ErrorMessage = "Employment type is required")]
    [StringLength(50, ErrorMessage = "Employment type cannot exceed 50 characters")]
    public string EmploymentType { get; set; } = string.Empty;

    /// <summary>
    /// Minimum salary
    /// </summary>
    [Range(0, 999999999.99, ErrorMessage = "Salary min must be between 0 and 999999999.99")]
    public decimal? SalaryMin { get; set; }

    /// <summary>
    /// Maximum salary
    /// </summary>
    [Range(0, 999999999.99, ErrorMessage = "Salary max must be between 0 and 999999999.99")]
    public decimal? SalaryMax { get; set; }

    /// <summary>
    /// Currency code (USD, THB, etc.)
    /// </summary>
    [StringLength(3, MinimumLength = 3, ErrorMessage = "Currency code must be exactly 3 characters")]
    [RegularExpression(@"^[A-Z]{3}$", ErrorMessage = "Currency code must be 3 uppercase letters")]
    public string? Currency { get; set; }

    /// <summary>
    /// Job description in Markdown format
    /// </summary>
    [Required(ErrorMessage = "Description is required")]
    [StringLength(10000, ErrorMessage = "Description cannot exceed 10000 characters")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Job requirements in Markdown format
    /// </summary>
    [Required(ErrorMessage = "Requirements are required")]
    [StringLength(10000, ErrorMessage = "Requirements cannot exceed 10000 characters")]
    public string Requirements { get; set; } = string.Empty;

    /// <summary>
    /// Job responsibilities in Markdown format
    /// </summary>
    [Required(ErrorMessage = "Responsibilities are required")]
    [StringLength(10000, ErrorMessage = "Responsibilities cannot exceed 10000 characters")]
    public string Responsibilities { get; set; } = string.Empty;

    /// <summary>
    /// Application deadline
    /// </summary>
    [Required(ErrorMessage = "Application deadline is required")]
    public DateTime ApplicationDeadline { get; set; }

    /// <summary>
    /// Whether to publish immediately
    /// </summary>
    public bool PublishImmediately { get; set; } = true;
}
