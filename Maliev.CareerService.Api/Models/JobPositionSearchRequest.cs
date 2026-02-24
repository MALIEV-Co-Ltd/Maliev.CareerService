using System.ComponentModel.DataAnnotations;

namespace Maliev.CareerService.Api.Models;
/// <summary>
/// Request model for jobpositionsearch
/// </summary>

public class JobPositionSearchRequest
{
    /// <summary>
    /// Gets or sets the page number for pagination.
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Page number must be greater than 0")]
    public int Page { get; set; } = 1;

    /// <summary>
    /// Gets or sets the number of items per page.
    /// </summary>
    [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// Gets or sets the job title filter.
    /// </summary>
    [MaxLength(200)]
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the department filter.
    /// </summary>
    [MaxLength(100)]
    public string? Department { get; set; }

    /// <summary>
    /// Gets or sets the employment type filter.
    /// </summary>
    [MaxLength(50)]
    public string? EmploymentType { get; set; }

    /// <summary>
    /// Gets or sets the experience level filter.
    /// </summary>
    [MaxLength(50)]
    public string? ExperienceLevel { get; set; }

    /// <summary>
    /// Gets or sets the list of work location IDs to filter by.
    /// </summary>
    public List<int> WorkLocationIds { get; set; } = [];
    /// <summary>
    /// Gets or sets the list of skill IDs to filter by.
    /// </summary>
    public List<int> SkillIds { get; set; } = [];

    /// <summary>
    /// Gets or sets the minimum salary filter.
    /// </summary>
    [Range(0, double.MaxValue)]
    public decimal? MinSalary { get; set; }

    /// <summary>
    /// Gets or sets the maximum salary filter.
    /// </summary>
    [Range(0, double.MaxValue)]
    public decimal? MaxSalary { get; set; }

    /// <summary>
    /// Gets or sets the currency code filter.
    /// </summary>
    [MaxLength(3)]
    public string? Currency { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to filter by active status.
    /// </summary>
    public bool? IsActive { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to filter by public visibility.
    /// </summary>
    public bool? IsPublic { get; set; }

    /// <summary>
    /// Gets or sets the search term for full-text search.
    /// </summary>
    [MaxLength(500)]
    public string? SearchTerm { get; set; }

    /// <summary>
    /// Gets or sets the field name to sort by.
    /// </summary>
    [MaxLength(50)]
    public string SortBy { get; set; } = "CreatedDate";

    /// <summary>
    /// Gets or sets a value indicating whether to sort in descending order.
    /// </summary>
    public bool SortDescending { get; set; } = true;
}
