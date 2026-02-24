using System.ComponentModel.DataAnnotations;

namespace Maliev.CareerService.Api.Models;
/// <summary>
/// Configuration options for Gcs
/// </summary>

public class GcsOptions
{
    /// <summary>
    /// Gets the configuration section name for Google Cloud Storage settings.
    /// </summary>
    public const string SectionName = "GoogleCloudStorage";

    /// <summary>
    /// Gets or sets the Google Cloud Storage bucket name.
    /// </summary>
    [Required]
    public required string BucketName { get; set; } = "maliev";

    /// <summary>
    /// Gets or sets the base path within the bucket.
    /// </summary>
    public string BasePath { get; set; } = "careers/applications";

    /// <summary>
    /// Gets or sets the expiration time in minutes for signed URLs.
    /// </summary>
    public int SignedUrlExpirationMinutes { get; set; } = 60;

    /// <summary>
    /// Gets or sets the maximum file size in bytes.
    /// </summary>
    public long MaxFileSizeBytes { get; set; } = 10 * 1024 * 1024; // 10MB

    /// <summary>
    /// Gets or sets the allowed MIME types for file uploads.
    /// </summary>
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
