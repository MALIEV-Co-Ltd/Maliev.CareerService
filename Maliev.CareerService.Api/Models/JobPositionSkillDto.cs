namespace Maliev.CareerService.Api.Models;

public class JobPositionSkillDto
{
    public int SkillId { get; set; }
    public required string SkillName { get; set; }
    public string? SkillCategory { get; set; }
    public required string RequiredLevel { get; set; }
    public bool IsRequired { get; set; }
}