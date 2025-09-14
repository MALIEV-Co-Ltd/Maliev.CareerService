namespace Maliev.CareerService.Api.Models;

public class JobPositionDto
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Department { get; set; }
    public required string Description { get; set; }
    public string? Requirements { get; set; }
    public string? Responsibilities { get; set; }
    public required string EmploymentType { get; set; }
    public required string ExperienceLevel { get; set; }
    public decimal? SalaryRangeMin { get; set; }
    public decimal? SalaryRangeMax { get; set; }
    public string? Currency { get; set; }
    public bool IsActive { get; set; }
    public bool IsPublic { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
    
    public List<WorkLocationDto> WorkLocations { get; set; } = new();
    public List<JobPositionSkillDto> Skills { get; set; } = new();
    public int ApplicationCount { get; set; }
}