using System.ComponentModel.DataAnnotations;

namespace Maliev.CareerService.Api.Models;

public class GcsOptions
{
    public const string SectionName = "GoogleCloudStorage";

    [Required]
    public required string BucketName { get; set; } = "maliev";

    public string BasePath { get; set; } = "careers/applications";

    public int SignedUrlExpirationMinutes { get; set; } = 60;

    public long MaxFileSizeBytes { get; set; } = 10 * 1024 * 1024; // 10MB

    public string[] AllowedMimeTypes { get; set; } =
    [
        "application/pdf",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "image/jpeg",
        "image/png",
        "image/gif"
    ];
}