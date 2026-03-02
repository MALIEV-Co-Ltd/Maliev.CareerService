using System.ComponentModel.DataAnnotations;

namespace Maliev.CareerService.Domain.Entities;

public class JobPosting : BaseEntity
{
    public string PositionTitle { get; set; } = string.Empty;

    public string PositionCode { get; set; } = string.Empty;

    public string? Department { get; set; }

    public string? Location { get; set; }

    public string EmploymentType { get; set; } = string.Empty;

    public decimal? SalaryMin { get; set; }

    public decimal? SalaryMax { get; set; }

    public string? Currency { get; set; }

    public string Description { get; set; } = string.Empty;

    public string Requirements { get; set; } = string.Empty;

    public string Responsibilities { get; set; } = string.Empty;

    public DateTime ApplicationDeadline { get; set; }

    public DateTime? PublishedAt { get; set; }

    public bool IsActive { get; set; }

    public ICollection<JobApplication> Applications { get; set; } = [];
}
