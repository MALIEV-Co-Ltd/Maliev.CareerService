using System.ComponentModel.DataAnnotations;

namespace Maliev.CareerService.Data.Entities;

public class JobApplication
{
    public int Id { get; set; }

    public int JobPositionId { get; set; }

    [Required]
    [MaxLength(255)]
    [EmailAddress]
    public required string ApplicantEmail { get; set; }

    [Required]
    [MaxLength(200)]
    public required string ApplicantName { get; set; }

    [MaxLength(50)]
    public string? ApplicantPhone { get; set; }

    [MaxLength(500)]
    public string? LinkedInProfile { get; set; }

    [MaxLength(500)]
    public string? PortfolioUrl { get; set; }

    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "Submitted";

    public DateTime ApplicationDate { get; set; } = DateTime.UtcNow;

    public DateTime LastStatusChange { get; set; } = DateTime.UtcNow;

    public string? Notes { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public JobPosition JobPosition { get; set; } = null!;
    public ICollection<ApplicationDocument> ApplicationDocuments { get; set; } = [];
}