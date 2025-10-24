using System.ComponentModel.DataAnnotations;

namespace Maliev.CareerService.Api.Models;

public class JobPositionSearchRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Page number must be greater than 0")]
    public int Page { get; set; } = 1;

    [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
    public int PageSize { get; set; } = 20;

    [MaxLength(200)]
    public string? Title { get; set; }

    [MaxLength(100)]
    public string? Department { get; set; }

    [MaxLength(50)]
    public string? EmploymentType { get; set; }

    [MaxLength(50)]
    public string? ExperienceLevel { get; set; }

    public List<int> WorkLocationIds { get; set; } = [];
    public List<int> SkillIds { get; set; } = [];

    [Range(0, double.MaxValue)]
    public decimal? MinSalary { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? MaxSalary { get; set; }

    [MaxLength(3)]
    public string? Currency { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsPublic { get; set; }

    [MaxLength(500)]
    public string? SearchTerm { get; set; }

    [MaxLength(50)]
    public string SortBy { get; set; } = "CreatedDate";

    public bool SortDescending { get; set; } = true;
}