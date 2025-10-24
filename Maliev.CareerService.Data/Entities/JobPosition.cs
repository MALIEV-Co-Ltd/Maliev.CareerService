using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maliev.CareerService.Data.Entities;

public class JobPosition
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public required string Title { get; set; }

    [Required]
    [MaxLength(100)]
    public required string Department { get; set; }

    [Required]
    public required string Description { get; set; }

    public string? Requirements { get; set; }

    public string? Responsibilities { get; set; }

    [Required]
    [MaxLength(50)]
    public required string EmploymentType { get; set; }

    [Required]
    [MaxLength(50)]
    public required string ExperienceLevel { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal? SalaryRangeMin { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal? SalaryRangeMax { get; set; }

    [MaxLength(3)]
    public string? Currency { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsPublic { get; set; } = true;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<JobApplication> JobApplications { get; set; } = [];
    public ICollection<JobPositionLocation> JobPositionLocations { get; set; } = [];
    public ICollection<JobPositionSkill> JobPositionSkills { get; set; } = [];
}