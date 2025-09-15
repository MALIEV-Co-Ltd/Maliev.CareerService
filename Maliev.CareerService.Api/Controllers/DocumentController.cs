using Asp.Versioning;
using Maliev.CareerService.Api.Models;
using Maliev.CareerService.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Maliev.CareerService.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("careers/v{version:apiVersion}/applications/{applicationId:int}/documents")]
[EnableRateLimiting("CareerPolicy")]
public class DocumentController : ControllerBase
{
    private readonly IDocumentService _documentService;
    private readonly ILogger<DocumentController> _logger;

    public DocumentController(
        IDocumentService documentService,
        ILogger<DocumentController> logger)
    {
        _documentService = documentService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all documents for a specific job application.
    /// </summary>
    /// <param name="applicationId">The ID of the job application.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of documents for the specified job application.</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET careers/v1.0/applications/1/documents
    ///
    /// Sample response:
    ///
    ///     [
    ///         {
    ///             "id": 1,
    ///             "jobApplicationId": 1,
    ///             "documentType": "Resume",
    ///             "originalFileName": "john_doe_resume.pdf",
    ///             "mimeType": "application/pdf",
    ///             "fileSize": 102400,
    ///             "description": "John Doe's resume",
    ///             "isRequired": true,
    ///             "displayOrder": 1,
    ///             "uploadDate": "2025-09-15T10:30:00Z",
    ///             "gcsBucket": "maliev-career-applications",
    ///             "gcsObjectName": "applications/000001/a1b2c3d4e5f6",
    ///             "gcsUri": "gs://maliev-career-applications/applications/000001/a1b2c3d4e5f6"
    ///         },
    ///         {
    ///             "id": 2,
    ///             "jobApplicationId": 1,
    ///             "documentType": "CoverLetter",
    ///             "originalFileName": "john_doe_cover_letter.pdf",
    ///             "mimeType": "application/pdf",
    ///             "fileSize": 51200,
    ///             "description": "John Doe's cover letter",
    ///             "isRequired": true,
    ///             "displayOrder": 2,
    ///             "uploadDate": "2025-09-15T10:30:00Z",
    ///             "gcsBucket": "maliev-career-applications",
    ///             "gcsObjectName": "applications/000001/b2c3d4e5f6a7",
    ///             "gcsUri": "gs://maliev-career-applications/applications/000001/b2c3d4e5f6a7"
    ///         }
    ///     ]
    ///
    /// Authentication:
    ///
    /// This endpoint requires authentication with a valid JWT token.
    ///
    /// Error responses:
    ///
    /// 401 Unauthorized - When the request is not authenticated
    /// 500 Internal Server Error - When there is an unexpected error
    /// </remarks>
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<ApplicationDocumentDto>>> GetDocumentsByApplication(
        int applicationId,
        CancellationToken cancellationToken = default)
    {
        var documents = await _documentService.GetByApplicationIdAsync(applicationId, cancellationToken);
        return Ok(documents);
    }

    /// <summary>
    /// Gets a document by its ID for a specific job application.
    /// </summary>
    /// <param name="applicationId">The ID of the job application.</param>
    /// <param name="documentId">The ID of the document to retrieve.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The document with the specified ID, or NotFound if not found.</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET careers/v1.0/applications/1/documents/1
    ///
    /// Sample response:
    ///
    ///     {
    ///         "id": 1,
    ///         "jobApplicationId": 1,
    ///         "documentType": "Resume",
    ///         "originalFileName": "john_doe_resume.pdf",
    ///         "mimeType": "application/pdf",
    ///         "fileSize": 102400,
    ///         "description": "John Doe's resume",
    ///         "isRequired": true,
    ///         "displayOrder": 1,
    ///         "uploadDate": "2025-09-15T10:30:00Z",
    ///         "gcsBucket": "maliev-career-applications",
    ///         "gcsObjectName": "applications/000001/a1b2c3d4e5f6",
    ///         "gcsUri": "gs://maliev-career-applications/applications/000001/a1b2c3d4e5f6"
    ///     }
    ///
    /// Authentication:
    ///
    /// This endpoint requires authentication with a valid JWT token.
    ///
    /// Error responses:
    ///
    /// 401 Unauthorized - When the request is not authenticated
    /// 404 Not Found - When the document with the specified ID does not exist for the application
    /// 500 Internal Server Error - When there is an unexpected error
    /// </remarks>
    [HttpGet("{documentId:int}")]
    [Authorize]
    public async Task<ActionResult<ApplicationDocumentDto>> GetDocument(
        int applicationId, 
        int documentId, 
        CancellationToken cancellationToken = default)
    {
        var document = await _documentService.GetByIdAsync(documentId, cancellationToken);
        
        if (document == null || document.JobApplicationId != applicationId)
        {
            return NotFound($"Document with ID {documentId} not found for application {applicationId}");
        }

        return Ok(document);
    }

    /// <summary>
    /// Uploads a document for a job application.
    /// </summary>
    /// <param name="applicationId">The ID of the job application.</param>
    /// <param name="file">The file to upload.</param>
    /// <param name="documentType">The type of document (e.g., Resume, CoverLetter, Portfolio).</param>
    /// <param name="description">Optional description of the document.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The uploaded document information.</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST careers/v1.0/applications/1/documents
    ///     Content-Type: multipart/form-data
    ///
    ///     file: [binary file data]
    ///     documentType: Resume
    ///     description: John Doe's resume
    ///
    /// Sample response:
    ///
    ///     {
    ///         "id": 1,
    ///         "jobApplicationId": 1,
    ///         "documentType": "Resume",
    ///         "originalFileName": "john_doe_resume.pdf",
    ///         "mimeType": "application/pdf",
    ///         "fileSize": 102400,
    ///         "description": "John Doe's resume",
    ///         "isRequired": true,
    ///         "displayOrder": 1,
    ///         "uploadDate": "2025-09-15T10:30:00Z",
    ///         "gcsBucket": "maliev-career-applications",
    ///         "gcsObjectName": "applications/000001/a1b2c3d4e5f6",
    ///         "gcsUri": "gs://maliev-career-applications/applications/000001/a1b2c3d4e5f6"
    ///     }
    ///
    /// Form parameters:
    ///
    /// - file: Required. The file to upload (max 10MB)
    /// - documentType: Required. The type of document (Resume, CoverLetter, Portfolio, etc.)
    /// - description: Optional. Description of the document (max 500 characters)
    ///
    /// Authentication:
    ///
    /// This endpoint requires authentication with a valid JWT token.
    ///
    /// Error responses:
    ///
    /// 400 Bad Request - When the file is missing, too large, or invalid
    /// 401 Unauthorized - When the request is not authenticated
    /// 500 Internal Server Error - When there is an unexpected error
    /// </remarks>
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ApplicationDocumentDto>> UploadDocument(
        int applicationId,
        IFormFile file,
        [FromForm] string documentType,
        [FromForm] string? description = null,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Create a request object from the form parameters
        var request = new DocumentUploadRequest
        {
            File = file,
            DocumentType = documentType,
            Description = description
        };

        try
        {
            var result = await _documentService.UploadDocumentAsync(applicationId, request, cancellationToken);
            
            _logger.LogInformation("Document uploaded for application {ApplicationId}, document ID {DocumentId}", 
                applicationId, result.Id);
            
            return CreatedAtAction(
                nameof(GetDocument),
                new { applicationId, documentId = result.Id },
                result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading document for application {ApplicationId}", applicationId);
            return StatusCode(500, "An error occurred while uploading the document");
        }
    }

    /// <summary>
    /// Downloads a document by its ID for a specific job application.
    /// </summary>
    /// <param name="applicationId">The ID of the job application.</param>
    /// <param name="documentId">The ID of the document to download.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The document file content.</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET careers/v1.0/applications/1/documents/1/download
    ///
    /// Sample response:
    ///
    ///     [binary file content]
    ///
    /// Authentication:
    ///
    /// This endpoint requires authentication with a valid JWT token.
    ///
    /// Error responses:
    ///
    /// 401 Unauthorized - When the request is not authenticated
    /// 404 Not Found - When the document with the specified ID does not exist for the application
    /// 500 Internal Server Error - When there is an unexpected error
    /// </remarks>
    [HttpGet("{documentId:int}/download")]
    [Authorize]
    public async Task<IActionResult> DownloadDocument(
        int applicationId,
        int documentId,
        CancellationToken cancellationToken = default)
    {
        // Verify document belongs to application
        var document = await _documentService.GetByIdAsync(documentId, cancellationToken);
        if (document == null || document.JobApplicationId != applicationId)
        {
            return NotFound($"Document with ID {documentId} not found for application {applicationId}");
        }

        var downloadResponse = await _documentService.GetDownloadUrlAsync(documentId, cancellationToken);
        
        if (downloadResponse == null)
        {
            return NotFound("Document file not found in storage");
        }

        return Ok(downloadResponse);
    }

    /// <summary>
    /// Deletes a document by its ID for a specific job application.
    /// </summary>
    /// <param name="applicationId">The ID of the job application.</param>
    /// <param name="documentId">The ID of the document to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>NoContent if successful, or NotFound if the document doesn't exist.</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     DELETE careers/v1.0/applications/1/documents/1
    ///
    /// Sample response:
    ///
    ///     204 No Content
    ///
    /// Authentication:
    ///
    /// This endpoint requires authentication with a valid JWT token.
    ///
    /// Error responses:
    ///
    /// 401 Unauthorized - When the request is not authenticated
    /// 404 Not Found - When the document with the specified ID does not exist for the application
    /// 500 Internal Server Error - When there is an unexpected error
    /// </remarks>
    [HttpDelete("{documentId:int}")]
    [Authorize]
    public async Task<ActionResult> DeleteDocument(
        int applicationId,
        int documentId,
        CancellationToken cancellationToken = default)
    {
        // Verify document belongs to application
        var document = await _documentService.GetByIdAsync(documentId, cancellationToken);
        if (document == null || document.JobApplicationId != applicationId)
        {
            return NotFound($"Document with ID {documentId} not found for application {applicationId}");
        }

        try
        {
            var success = await _documentService.DeleteAsync(documentId, cancellationToken);
            
            if (!success)
            {
                return NotFound($"Document with ID {documentId} not found");
            }

            _logger.LogInformation("Document {DocumentId} deleted from application {ApplicationId}", 
                documentId, applicationId);
            
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting document {DocumentId}", documentId);
            return StatusCode(500, "An error occurred while deleting the document");
        }
    }

    /// <summary>
    /// Checks if a document with the specified ID exists for a job application.
    /// </summary>
    /// <param name="applicationId">The ID of the job application.</param>
    /// <param name="documentId">The ID of the document to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the document exists, false otherwise.</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET careers/v1.0/applications/1/documents/1/exists
    ///
    /// Sample response:
    ///
    ///     true
    ///
    /// Error responses:
    ///
    /// 500 Internal Server Error - When there is an unexpected error
    /// </remarks>
    [HttpGet("{documentId:int}/exists")]
    [Authorize]
    public async Task<ActionResult<bool>> CheckDocumentExists(
        int applicationId,
        int documentId,
        CancellationToken cancellationToken = default)
    {
        // Check if the document exists and belongs to the specified application
        var document = await _documentService.GetByIdAsync(documentId, cancellationToken);
        var exists = document != null && document.JobApplicationId == applicationId;
        return Ok(exists);
    }

    /// <summary>
    /// Gets the total file size of all documents for a job application.
    /// </summary>
    /// <param name="applicationId">The ID of the job application.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The total file size in bytes.</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET careers/v1.0/applications/1/documents/total-size
    ///
    /// Sample response:
    ///
    ///     153600
    ///
    /// Error responses:
    ///
    /// 500 Internal Server Error - When there is an unexpected error
    /// </remarks>
    [HttpGet("total-size")]
    [Authorize]
    public async Task<ActionResult<long>> GetTotalFileSize(
        int applicationId,
        CancellationToken cancellationToken = default)
    {
        var totalSize = await _documentService.GetTotalFileSizeByApplicationAsync(applicationId, cancellationToken);
        return Ok(totalSize);
    }
}