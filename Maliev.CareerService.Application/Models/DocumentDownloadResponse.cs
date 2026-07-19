namespace Maliev.CareerService.Application.Models;
/// <summary>
/// Response model for documentdownload
/// </summary>

public class DocumentDownloadResponse
{
    /// <summary>
    /// Gets or sets the temporary download URL for the document.
    /// </summary>
    public required string DownloadUrl { get; set; }
    /// <summary>
    /// Gets or sets the original file name of the document.
    /// </summary>
    public required string OriginalFileName { get; set; }
    /// <summary>
    /// Gets or sets the MIME type of the document.
    /// </summary>
    public required string MimeType { get; set; }
    /// <summary>
    /// Gets or sets the file size in bytes.
    /// </summary>
    public long FileSize { get; set; }
    /// <summary>
    /// Gets or sets the expiration time for the download URL.
    /// </summary>
    public DateTime DownloadUrlExpiration { get; set; }
}
