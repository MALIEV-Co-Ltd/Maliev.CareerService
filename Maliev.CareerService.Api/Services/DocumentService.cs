using Maliev.CareerService.Api.Models;
using Maliev.CareerService.Data.DbContexts;
using Maliev.CareerService.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Maliev.CareerService.Api.Services;

public class DocumentService : IDocumentService
{
    private readonly CareerDbContext _context;
    private readonly IUploadServiceClient _uploadServiceClient;
    private readonly ILogger<DocumentService> _logger;
    private readonly GcsConfiguration _gcsConfiguration;
    private readonly IFileValidationService _fileValidationService;

    private readonly HashSet<string> _allowedDocumentTypes = new()
    {
        DocumentType.Resume,
        DocumentType.CoverLetter,
        DocumentType.Portfolio,
        DocumentType.Certificate,
        DocumentType.Transcript,
        DocumentType.Other
    };

    private readonly HashSet<string> _allowedMimeTypes = new()
    {
        "application/pdf",
        "text/plain",
        "image/jpeg",
        "image/png",
        "application/zip"
    };

    private const long MaxFileSize = 10 * 1024 * 1024; // 10MB
    private const long MaxTotalSizePerApplication = 50 * 1024 * 1024; // 50MB

    public DocumentService(
        CareerDbContext context,
        IUploadServiceClient uploadServiceClient,
        ILogger<DocumentService> logger,
        IOptions<GcsConfiguration> gcsConfiguration,
        IFileValidationService fileValidationService)
    {
        _context = context;
        _uploadServiceClient = uploadServiceClient;
        _logger = logger;
        _gcsConfiguration = gcsConfiguration.Value;
        _fileValidationService = fileValidationService;
    }

    public async Task<ApplicationDocumentDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var document = await _context.ApplicationDocuments
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

        return document == null ? null : MapToDto(document);
    }

    public async Task<IEnumerable<ApplicationDocumentDto>> GetByApplicationIdAsync(int applicationId, CancellationToken cancellationToken = default)
    {
        var documents = await _context.ApplicationDocuments
            .Where(d => d.JobApplicationId == applicationId)
            .OrderBy(d => d.DisplayOrder)
            .ThenBy(d => d.UploadDate)
            .ToListAsync(cancellationToken);

        return documents.Select(MapToDto);
    }

    public async Task<ApplicationDocumentDto> UploadDocumentAsync(int applicationId, DocumentUploadRequest request, CancellationToken cancellationToken = default)
    {
        // Validate request
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (request.File == null)
        {
            throw new ArgumentException("File is required", nameof(request.File));
        }

        // Validate application exists
        var applicationExists = await _context.JobApplications
            .AnyAsync(ja => ja.Id == applicationId, cancellationToken);

        if (!applicationExists)
        {
            throw new ArgumentException("Job application not found", nameof(applicationId));
        }

        // Validate document type
        if (string.IsNullOrEmpty(request.DocumentType))
        {
            throw new ArgumentException("Document type is required", nameof(request.DocumentType));
        }

        if (!_allowedDocumentTypes.Contains(request.DocumentType))
        {
            throw new ArgumentException($"Invalid document type '{request.DocumentType}'", nameof(request.DocumentType));
        }

        // Validate document type and MIME type using the new validation service
        if (!_fileValidationService.IsMimeTypeAllowed(request.File.ContentType))
        {
            throw new ArgumentException($"Invalid MIME type '{request.File.ContentType}'");
        }

        if (!_fileValidationService.IsFileExtensionAllowed(request.File.FileName))
        {
            throw new ArgumentException($"Invalid file extension '{Path.GetExtension(request.File.FileName)}'");
        }

        // Validate file size using the DocumentService's limit
        if (request.File.Length > MaxFileSize)
        {
            throw new ArgumentException($"File size {request.File.Length} bytes exceeds maximum allowed size of {MaxFileSize} bytes");
        }

        // Check total size limit for application
        var currentTotalSize = await GetTotalFileSizeByApplicationAsync(applicationId, cancellationToken);
        if (currentTotalSize + request.File.Length > MaxTotalSizePerApplication)
        {
            throw new ArgumentException($"Adding this file would exceed the total size limit of {MaxTotalSizePerApplication} bytes per application");
        }

        // Validate file content using the new validation service
        FileValidationResult validationResult;
        using (var fileStream = request.File.OpenReadStream())
        {
            validationResult = await _fileValidationService.ValidateFileAsync(
                fileStream, request.File.FileName, request.File.ContentType, request.File.Length);
        }
        
        if (!validationResult.IsValid)
        {
            throw new ArgumentException($"File validation failed: {validationResult.ErrorMessage}");
        }

        // Generate upload path
        var uploadPath = GenerateUploadPath(applicationId);

        // Upload file to UploadService
        UploadServiceResponse uploadResult;
        using (var fileStream = request.File.OpenReadStream())
        {
            var uploadRequest = new UploadServiceRequest
            {
                FileName = request.File.FileName,
                ContentType = request.File.ContentType,
                UploadPath = uploadPath,
                FileSize = request.File.Length,
                Metadata = new Dictionary<string, string>
                {
                    ["applicationId"] = applicationId.ToString(),
                    ["documentType"] = request.DocumentType,
                    ["description"] = request.Description ?? string.Empty,
                    ["fileHash"] = validationResult.FileHash ?? string.Empty
                }
            };

            uploadResult = await _uploadServiceClient.UploadFileAsync(fileStream, uploadRequest, cancellationToken);
        }

        // Create document record in database
        var document = new ApplicationDocument
        {
            JobApplicationId = applicationId,
            DocumentType = request.DocumentType,
            OriginalFileName = request.File.FileName,
            MimeType = request.File.ContentType,
            FileSize = uploadResult.Size,
            Description = request.Description,
            IsRequired = request.IsRequired,
            DisplayOrder = request.DisplayOrder,
            GcsBucket = uploadResult.Bucket,
            GcsObjectName = uploadResult.ObjectName,
            GcsUri = uploadResult.Uri
        };

        _context.ApplicationDocuments.Add(document);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Uploaded document {DocumentId} for application {ApplicationId} to {ObjectName}", 
            document.Id, applicationId, uploadResult.ObjectName);

        return MapToDto(document);
    }

    public async Task<DocumentDownloadResponse?> GetDownloadUrlAsync(int id, CancellationToken cancellationToken = default)
    {
        var document = await _context.ApplicationDocuments
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

        if (document == null)
        {
            return null;
        }

        // Get download URL from UploadService
        var downloadUrl = await _uploadServiceClient.GetDownloadUrlAsync(
            document.GcsBucket, 
            document.GcsObjectName, 
            TimeSpan.FromHours(1), 
            cancellationToken);

        if (downloadUrl == null)
        {
            _logger.LogWarning("Failed to generate download URL for document {DocumentId}", id);
            return null;
        }

        return new DocumentDownloadResponse
        {
            DownloadUrl = downloadUrl,
            OriginalFileName = document.OriginalFileName,
            MimeType = document.MimeType,
            FileSize = document.FileSize,
            DownloadUrlExpiration = DateTime.UtcNow.AddHours(1)
        };
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var document = await _context.ApplicationDocuments
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

        if (document == null)
        {
            return false;
        }

        // Delete from UploadService/GCS
        var deleteSuccess = await _uploadServiceClient.DeleteFileAsync(
            document.GcsBucket, 
            document.GcsObjectName, 
            cancellationToken);

        if (!deleteSuccess)
        {
            _logger.LogWarning("Failed to delete document {DocumentId} from storage, but will remove database record", id);
        }

        // Delete from database regardless of storage deletion result
        _context.ApplicationDocuments.Remove(document);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted document {DocumentId} from database", id);
        return true;
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.ApplicationDocuments
            .AnyAsync(d => d.Id == id, cancellationToken);
    }

    public async Task<long> GetTotalFileSizeByApplicationAsync(int applicationId, CancellationToken cancellationToken = default)
    {
        return await _context.ApplicationDocuments
            .Where(d => d.JobApplicationId == applicationId)
            .SumAsync(d => d.FileSize, cancellationToken);
    }

    private string GenerateUploadPath(int applicationId)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd");
        return $"{_gcsConfiguration.BasePath}/applications/{applicationId}/{timestamp}";
    }

    private static ApplicationDocumentDto MapToDto(ApplicationDocument document)
    {
        return new ApplicationDocumentDto
        {
            Id = document.Id,
            JobApplicationId = document.JobApplicationId,
            DocumentType = document.DocumentType,
            OriginalFileName = document.OriginalFileName,
            GcsBucket = document.GcsBucket,
            GcsObjectName = document.GcsObjectName,
            GcsUri = document.GcsUri,
            FileSize = document.FileSize,
            MimeType = document.MimeType,
            UploadDate = document.UploadDate,
            IsRequired = document.IsRequired,
            DisplayOrder = document.DisplayOrder,
            Description = document.Description
        };
    }
}