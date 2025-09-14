namespace Maliev.CareerService.Api.Models;

public class WorkLocationDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Address { get; set; }
    public required string City { get; set; }
    public int? CountryId { get; set; }
    public bool IsRemoteAllowed { get; set; }
    public bool IsHybrid { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
}