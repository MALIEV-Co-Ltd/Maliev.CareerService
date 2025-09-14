using System.ComponentModel.DataAnnotations;

namespace Maliev.CareerService.Api.Models;

public class DocumentUploadRequest
{
    [Required]
    public required IFormFile File { get; set; }
    
    [Required]
    [MaxLength(50)]
    public required string DocumentType { get; set; }
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    public bool IsRequired { get; set; } = false;
    
    public int DisplayOrder { get; set; } = 0;
}