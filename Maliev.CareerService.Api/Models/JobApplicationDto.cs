namespace Maliev.CareerService.Api.Models;

public class JobApplicationDto
{
    public int Id { get; set; }
    public int JobPositionId { get; set; }
    public required string ApplicantEmail { get; set; }
    public required string ApplicantName { get; set; }
    public string? ApplicantPhone { get; set; }
    public string? LinkedInProfile { get; set; }
    public string? PortfolioUrl { get; set; }
    public required string Status { get; set; }
    public DateTime ApplicationDate { get; set; }
    public DateTime LastStatusChange { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
    
    public JobPositionDto? JobPosition { get; set; }
    public List<ApplicationDocumentDto> Documents { get; set; } = new();
}