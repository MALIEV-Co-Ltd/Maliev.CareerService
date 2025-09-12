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

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<ApplicationDocumentDto>>> GetApplicationDocuments(
        int applicationId, 
        CancellationToken cancellationToken = default)
    {
        var documents = await _documentService.GetByApplicationIdAsync(applicationId, cancellationToken);
        return Ok(documents);
    }

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

    [HttpPost]
    [AllowAnonymous] // Allow applicants to upload documents
    [EnableRateLimiting("GlobalPolicy")]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10MB limit
    public async Task<ActionResult<ApplicationDocumentDto>> UploadDocument(
        int applicationId,
        [FromForm] DocumentUploadRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

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

    [HttpGet("{documentId:int}/download")]
    [Authorize]
    public async Task<ActionResult<DocumentDownloadResponse>> GetDownloadUrl(
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

    [HttpGet("{documentId:int}/exists")]
    [Authorize]
    public async Task<ActionResult<bool>> CheckDocumentExists(
        int applicationId, 
        int documentId, 
        CancellationToken cancellationToken = default)
    {
        var document = await _documentService.GetByIdAsync(documentId, cancellationToken);
        var exists = document != null && document.JobApplicationId == applicationId;
        return Ok(exists);
    }

    [HttpGet("size")]
    [Authorize]
    public async Task<ActionResult<long>> GetTotalFileSize(
        int applicationId, 
        CancellationToken cancellationToken = default)
    {
        var totalSize = await _documentService.GetTotalFileSizeByApplicationAsync(applicationId, cancellationToken);
        return Ok(totalSize);
    }
}