namespace Maliev.CareerService.Api.Models;

public class SkillDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Category { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
}