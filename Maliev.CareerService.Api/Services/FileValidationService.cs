using System.Security.Cryptography;
using System.Text;

namespace Maliev.CareerService.Api.Services;

public interface IFileValidationService
{
    Task<FileValidationResult> ValidateFileAsync(Stream fileStream, string fileName, string mimeType, long fileSize);
    bool IsMimeTypeAllowed(string mimeType);
    bool IsFileExtensionAllowed(string fileName);
    bool IsFileSizeWithinLimit(long fileSize);
}

public class FileValidationService : IFileValidationService
{
    private readonly HashSet<string> _allowedMimeTypes = new()
    {
        // Documents
        "application/pdf",
        "text/plain",
        "application/msword", // .doc
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document", // .docx
        "application/vnd.ms-excel", // .xls
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", // .xlsx
        "application/vnd.ms-powerpoint", // .ppt
        "application/vnd.openxmlformats-officedocument.presentationml.presentation", // .pptx
        
        // Images
        "image/jpeg",
        "image/png",
        "image/gif",
        "image/bmp",
        "image/tiff",
        
        // Archives
        "application/zip",
        "application/x-rar-compressed",
        "application/x-7z-compressed"
    };

    private readonly HashSet<string> _allowedExtensions = new()
    {
        // Documents
        ".pdf", ".txt", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx",
        
        // Images
        ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff", ".tif",
        
        // Archives
        ".zip", ".rar", ".7z"
    };

    private const long MaxFileSize = 50 * 1024 * 1024; // 50MB
    private const long MaxTotalSizePerApplication = 100 * 1024 * 1024; // 100MB

    public bool IsMimeTypeAllowed(string mimeType)
    {
        return _allowedMimeTypes.Contains(mimeType.ToLowerInvariant());
    }

    public bool IsFileExtensionAllowed(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return _allowedExtensions.Contains(extension);
    }

    public bool IsFileSizeWithinLimit(long fileSize)
    {
        return fileSize > 0 && fileSize <= MaxFileSize;
    }

    public async Task<FileValidationResult> ValidateFileAsync(Stream fileStream, string fileName, string mimeType, long fileSize)
    {
        var result = new FileValidationResult { IsValid = true };

        // Check file size
        if (!IsFileSizeWithinLimit(fileSize))
        {
            result.IsValid = false;
            result.ErrorMessage = $"File size {fileSize} bytes exceeds maximum allowed size of {MaxFileSize} bytes";
            return result;
        }

        // Check file extension
        if (!IsFileExtensionAllowed(fileName))
        {
            result.IsValid = false;
            result.ErrorMessage = $"File extension {Path.GetExtension(fileName)} is not allowed";
            return result;
        }

        // Check MIME type
        if (!IsMimeTypeAllowed(mimeType))
        {
            result.IsValid = false;
            result.ErrorMessage = $"MIME type {mimeType} is not allowed";
            return result;
        }

        // Validate actual file content (magic number validation)
        var contentValidation = await ValidateFileContentAsync(fileStream, mimeType);
        if (!contentValidation.IsValid)
        {
            result.IsValid = false;
            result.ErrorMessage = contentValidation.ErrorMessage;
            return result;
        }

        // Calculate file hash for security
        result.FileHash = await CalculateFileHashAsync(fileStream);

        return result;
    }

    private async Task<FileValidationResult> ValidateFileContentAsync(Stream fileStream, string mimeType)
    {
        var result = new FileValidationResult { IsValid = true };

        // Save current position
        var originalPosition = fileStream.Position;

        try
        {
            // Reset to beginning of stream
            fileStream.Position = 0;

            // Read first few bytes to check file signature (magic numbers)
            var buffer = new byte[8];
            var bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length);
            
            if (bytesRead < 4)
            {
                result.IsValid = false;
                result.ErrorMessage = "File is too small to be valid";
                return result;
            }

            // Check file signatures
            var fileSignature = BitConverter.ToString(buffer).Replace("-", "");
            
            bool isValid = mimeType switch
            {
                "application/pdf" => fileSignature.StartsWith("25504446"), // %PDF
                "image/jpeg" => fileSignature.StartsWith("FFD8FFE0") || fileSignature.StartsWith("FFD8FFE1"), // JPEG
                "image/png" => fileSignature.StartsWith("89504E47"), // PNG
                "image/gif" => fileSignature.StartsWith("47494638"), // GIF
                "application/zip" => fileSignature.StartsWith("504B0304") || fileSignature.StartsWith("504B0506") || fileSignature.StartsWith("504B0708"), // ZIP
                "application/msword" => fileSignature.StartsWith("D0CF11E0"), // DOC
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document" => fileSignature.StartsWith("504B0304"), // DOCX (ZIP-based)
                _ => true // For other types, we'll allow them if extension and MIME type match
            };

            if (!isValid)
            {
                result.IsValid = false;
                result.ErrorMessage = $"File content does not match the declared MIME type {mimeType}";
            }

            return result;
        }
        finally
        {
            // Restore original position
            fileStream.Position = originalPosition;
        }
    }

    private async Task<string> CalculateFileHashAsync(Stream fileStream)
    {
        var originalPosition = fileStream.Position;
        
        try
        {
            fileStream.Position = 0;
            using var sha256 = SHA256.Create();
            var hash = await sha256.ComputeHashAsync(fileStream);
            return Convert.ToBase64String(hash);
        }
        finally
        {
            fileStream.Position = originalPosition;
        }
    }
}

public class FileValidationResult
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public string? FileHash { get; set; }
}