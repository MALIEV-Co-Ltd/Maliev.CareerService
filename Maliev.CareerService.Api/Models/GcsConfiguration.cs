using System.ComponentModel.DataAnnotations;

namespace Maliev.CareerService.Api.Models;

public class GcsConfiguration
{
    public const string SectionName = "Gcs";

    [Required]
    public required string BasePath { get; set; }
}