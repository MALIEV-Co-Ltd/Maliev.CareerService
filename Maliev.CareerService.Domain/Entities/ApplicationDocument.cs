using System.ComponentModel.DataAnnotations;

namespace Maliev.CareerService.Domain.Entities;

public class ApplicationDocument
{
    public int Id { get; set; }

    public int JobApplicationId { get; set; }

    [Required]
    [MaxLength(50)]
    public required string DocumentType { get; set; }

    [Required]
    [MaxLength(255)]
    public required string OriginalFileName { get; set; }

    [Required]
    [MaxLength(100)]
    public required string GcsBucket { get; set; }

    [Required]
    [MaxLength(500)]
    public required string GcsObjectName { get; set; }

    [Required]
    [MaxLength(1000)]
    public required string GcsUri { get; set; }

    public long FileSize { get; set; }

    [Required]
    [MaxLength(100)]
    public required string MimeType { get; set; }

    public DateTime UploadDate { get; set; } = DateTime.UtcNow;

    public bool IsRequired { get; set; } = false;

    public int DisplayOrder { get; set; } = 0;

    [MaxLength(500)]
    public string? Description { get; set; }

    public JobApplication JobApplication { get; set; } = null!;
}
