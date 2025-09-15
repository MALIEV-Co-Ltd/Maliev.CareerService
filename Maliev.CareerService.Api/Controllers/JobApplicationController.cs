using Asp.Versioning;
using Maliev.CareerService.Api.Models;
using Maliev.CareerService.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Maliev.CareerService.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("careers/v{version:apiVersion}/applications")]
[EnableRateLimiting("CareerPolicy")]
public class JobApplicationController : ControllerBase
{
    private readonly IJobApplicationService _jobApplicationService;
    private readonly ILogger<JobApplicationController> _logger;

    public JobApplicationController(
        IJobApplicationService jobApplicationService,
        ILogger<JobApplicationController> logger)
    {
        _jobApplicationService = jobApplicationService;
        _logger = logger;
    }

    /// <summary>
    /// Gets a job application by its ID.
    /// </summary>
    /// <param name="id">The ID of the job application to retrieve.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The job application with the specified ID, or NotFound if not found.</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET careers/v1.0/applications/1
    ///
    /// Sample response:
    ///
    ///     {
    ///         "id": 1,
    ///         "jobPositionId": 1,
    ///         "applicantName": "John Doe",
    ///         "applicantEmail": "john.doe@example.com",
    ///         "applicantPhone": "+1234567890",
    ///         "linkedInProfile": "https://linkedin.com/in/johndoe",
    ///         "portfolioUrl": "https://johndoe.com/portfolio",
    ///         "coverLetterText": "I'm excited to apply for this position...",
    ///         "status": "Submitted",
    ///         "applicationDate": "2025-09-15T10:30:00Z",
    ///         "lastStatusChange": "2025-09-15T10:30:00Z",
    ///         "documents": [
    ///             {
    ///                 "id": 1,
    ///                 "jobApplicationId": 1,
    ///                 "documentType": "Resume",
    ///                 "originalFileName": "john_doe_resume.pdf",
    ///                 "mimeType": "application/pdf",
    ///                 "fileSize": 102400,
    ///                 "description": "John Doe's resume",
    ///                 "isRequired": true,
    ///                 "displayOrder": 1,
    ///                 "uploadDate": "2025-09-15T10:30:00Z",
    ///                 "gcsBucket": "maliev-career-applications",
    ///                 "gcsObjectName": "applications/000001/a1b2c3d4e5f6",
    ///                 "gcsUri": "gs://maliev-career-applications/applications/000001/a1b2c3d4e5f6"
    ///             }
    ///         ],
    ///         "notes": [
    ///             {
    ///                 "id": 1,
    ///                 "jobApplicationId": 1,
    ///                 "noteText": "Initial screening completed",
    ///                 "createdBy": "admin@example.com",
    ///                 "createdDate": "2025-09-15T11:00:00Z"
    ///             }
    ///         ]
    ///     }
    ///
    /// Authentication:
    ///
    /// This endpoint requires authentication with a valid JWT token.
    ///
    /// Error responses:
    ///
    /// 401 Unauthorized - When the request is not authenticated
    /// 404 Not Found - When the job application with the specified ID does not exist
    /// 500 Internal Server Error - When there is an unexpected error
    /// </remarks>
    [HttpGet("{id:int}")]
    [Authorize]
    public async Task<ActionResult<JobApplicationDto>> GetJobApplication(int id, CancellationToken cancellationToken = default)
    {
        var application = await _jobApplicationService.GetByIdAsync(id, cancellationToken);
        
        if (application == null)
        {
            return NotFound($"Job application with ID {id} not found");
        }

        return Ok(application);
    }

    /// <summary>
    /// Gets all job applications with optional filtering and pagination.
    /// </summary>
    /// <param name="page">Page number (default: 1).</param>
    /// <param name="pageSize">Page size (default: 20, max: 100).</param>
    /// <param name="status">Optional filter by application status.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paged result of job applications.</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET careers/v1.0/applications?page=1&amp;pageSize=20&amp;status=Submitted
    ///
    /// Sample response:
    ///
    ///     {
    ///         "items": [
    ///             {
    ///                 "id": 1,
    ///                 "jobPositionId": 1,
    ///                 "applicantName": "John Doe",
    ///                 "applicantEmail": "john.doe@example.com",
    ///                 "applicantPhone": "+1234567890",
    ///                 "linkedInProfile": "https://linkedin.com/in/johndoe",
    ///                 "portfolioUrl": "https://johndoe.com/portfolio",
    ///                 "coverLetterText": "I'm excited to apply for this position...",
    ///                 "status": "Submitted",
    ///                 "applicationDate": "2025-09-15T10:30:00Z",
    ///                 "lastStatusChange": "2025-09-15T10:30:00Z",
    ///                 "documents": [
    ///                     {
    ///                         "id": 1,
    ///                         "jobApplicationId": 1,
    ///                         "documentType": "Resume",
    ///                         "originalFileName": "john_doe_resume.pdf",
    ///                         "mimeType": "application/pdf",
    ///                         "fileSize": 102400,
    ///                         "description": "John Doe's resume",
    ///                         "isRequired": true,
    ///                         "displayOrder": 1,
    ///                         "uploadDate": "2025-09-15T10:30:00Z",
    ///                         "gcsBucket": "maliev-career-applications",
    ///                         "gcsObjectName": "applications/000001/a1b2c3d4e5f6",
    ///                         "gcsUri": "gs://maliev-career-applications/applications/000001/a1b2c3d4e5f6"
    ///                     }
    ///                 ],
    ///                 "notes": [
    ///                     {
    ///                         "id": 1,
    ///                         "jobApplicationId": 1,
    ///                         "noteText": "Initial screening completed",
    ///                         "createdBy": "admin@example.com",
    ///                         "createdDate": "2025-09-15T11:00:00Z"
    ///                     }
    ///                 ]
    ///             }
    ///         ],
    ///         "totalCount": 1,
    ///         "page": 1,
    ///         "pageSize": 20,
    ///         "totalPages": 1,
    ///         "hasPrevious": false,
    ///         "hasNext": false
    ///     }
    ///
    /// Query parameters:
    ///
    /// - page: Page number (default: 1)
    /// - pageSize: Number of items per page (default: 20, max: 100)
    /// - status: Optional filter by application status (Submitted, UnderReview, Interviewed, Accepted, Rejected)
    ///
    /// Authentication:
    ///
    /// This endpoint requires authentication with a valid JWT token.
    ///
    /// Error responses:
    ///
    /// 400 Bad Request - When the request parameters are invalid
    /// 401 Unauthorized - When the request is not authenticated
    /// 500 Internal Server Error - When there is an unexpected error
    /// </remarks>
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<PagedResult<JobApplicationDto>>> GetAllJobApplications(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null,
        CancellationToken cancellationToken = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var result = await _jobApplicationService.GetAllAsync(page, pageSize, status, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets job applications for a specific job position with optional filtering and pagination.
    /// </summary>
    /// <param name="jobPositionId">The ID of the job position.</param>
    /// <param name="page">Page number (default: 1).</param>
    /// <param name="pageSize">Page size (default: 20, max: 100).</param>
    /// <param name="status">Optional filter by application status.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paged result of job applications for the specified job position.</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET careers/v1.0/applications/position/1?page=1&amp;pageSize=20&amp;status=Submitted
    ///
    /// Sample response:
    ///
    ///     {
    ///         "items": [
    ///             {
    ///                 "id": 1,
    ///                 "jobPositionId": 1,
    ///                 "applicantName": "John Doe",
    ///                 "applicantEmail": "john.doe@example.com",
    ///                 "applicantPhone": "+1234567890",
    ///                 "linkedInProfile": "https://linkedin.com/in/johndoe",
    ///                 "portfolioUrl": "https://johndoe.com/portfolio",
    ///                 "coverLetterText": "I'm excited to apply for this position...",
    ///                 "status": "Submitted",
    ///                 "applicationDate": "2025-09-15T10:30:00Z",
    ///                 "lastStatusChange": "2025-09-15T10:30:00Z",
    ///                 "documents": [
    ///                     {
    ///                         "id": 1,
    ///                         "jobApplicationId": 1,
    ///                         "documentType": "Resume",
    ///                         "originalFileName": "john_doe_resume.pdf",
    ///                         "mimeType": "application/pdf",
    ///                         "fileSize": 102400,
    ///                         "description": "John Doe's resume",
    ///                         "isRequired": true,
    ///                         "displayOrder": 1,
    ///                         "uploadDate": "2025-09-15T10:30:00Z",
    ///                         "gcsBucket": "maliev-career-applications",
    ///                         "gcsObjectName": "applications/000001/a1b2c3d4e5f6",
    ///                         "gcsUri": "gs://maliev-career-applications/applications/000001/a1b2c3d4e5f6"
    ///                     }
    ///                 ],
    ///                 "notes": [
    ///                     {
    ///                         "id": 1,
    ///                         "jobApplicationId": 1,
    ///                         "noteText": "Initial screening completed",
    ///                         "createdBy": "admin@example.com",
    ///                         "createdDate": "2025-09-15T11:00:00Z"
    ///                     }
    ///                 ]
    ///             }
    ///         ],
    ///         "totalCount": 1,
    ///         "page": 1,
    ///         "pageSize": 20,
    ///         "totalPages": 1,
    ///         "hasPrevious": false,
    ///         "hasNext": false
    ///     }
    ///
    /// Query parameters:
    ///
    /// - page: Page number (default: 1)
    /// - pageSize: Number of items per page (default: 20, max: 100)
    /// - status: Optional filter by application status (Submitted, UnderReview, Interviewed, Accepted, Rejected)
    ///
    /// Authentication:
    ///
    /// This endpoint requires authentication with a valid JWT token.
    ///
    /// Error responses:
    ///
    /// 400 Bad Request - When the request parameters are invalid
    /// 401 Unauthorized - When the request is not authenticated
    /// 500 Internal Server Error - When there is an unexpected error
    /// </remarks>
    [HttpGet("position/{jobPositionId:int}")]
    [Authorize]
    public async Task<ActionResult<PagedResult<JobApplicationDto>>> GetApplicationsByJobPosition(
        int jobPositionId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null,
        CancellationToken cancellationToken = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var result = await _jobApplicationService.GetByJobPositionIdAsync(jobPositionId, page, pageSize, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets job applications for a specific applicant email.
    /// </summary>
    /// <param name="email">The applicant email.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of job applications for the specified applicant email.</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET careers/v1.0/applications/email/john.doe@example.com
    ///
    /// Sample response:
    ///
    ///     [
    ///         {
    ///             "id": 1,
    ///             "jobPositionId": 1,
    ///             "applicantName": "John Doe",
    ///             "applicantEmail": "john.doe@example.com",
    ///             "applicantPhone": "+1234567890",
    ///             "linkedInProfile": "https://linkedin.com/in/johndoe",
    ///             "portfolioUrl": "https://johndoe.com/portfolio",
    ///             "coverLetterText": "I'm excited to apply for this position...",
    ///             "status": "Submitted",
    ///             "applicationDate": "2025-09-15T10:30:00Z",
    ///             "lastStatusChange": "2025-09-15T10:30:00Z",
    ///             "documents": [
    ///                 {
    ///                     "id": 1,
    ///                     "jobApplicationId": 1,
    ///                     "documentType": "Resume",
    ///                     "originalFileName": "john_doe_resume.pdf",
    ///                     "mimeType": "application/pdf",
    ///                     "fileSize": 102400,
    ///                     "description": "John Doe's resume",
    ///                     "isRequired": true,
    ///                     "displayOrder": 1,
    ///                     "uploadDate": "2025-09-15T10:30:00Z",
    ///                     "gcsBucket": "maliev-career-applications",
    ///                     "gcsObjectName": "applications/000001/a1b2c3d4e5f6",
    ///                     "gcsUri": "gs://maliev-career-applications/applications/000001/a1b2c3d4e5f6"
    ///                 }
    ///             ],
    ///             "notes": [
    ///                 {
    ///                     "id": 1,
    ///                     "jobApplicationId": 1,
    ///                     "noteText": "Initial screening completed",
    ///                     "createdBy": "admin@example.com",
    ///                     "createdDate": "2025-09-15T11:00:00Z"
    ///                 }
    ///             ]
    ///         }
    ///     ]
    ///
    /// Authentication:
    ///
    /// This endpoint requires authentication with a valid JWT token.
    ///
    /// Error responses:
    ///
    /// 400 Bad Request - When the email parameter is invalid
    /// 401 Unauthorized - When the request is not authenticated
    /// 500 Internal Server Error - When there is an unexpected error
    /// </remarks>
    [HttpGet("email/{email}")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<JobApplicationDto>>> GetApplicationsByEmail(
        string email,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return BadRequest("Email is required");
        }

        var applications = await _jobApplicationService.GetByEmailAsync(email, cancellationToken);
        return Ok(applications);
    }

    /// <summary>
    /// Creates a new job application.
    /// </summary>
    /// <param name="request">The request containing job application details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created job application.</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST careers/v1.0/applications
    ///     {
    ///         "jobPositionId": 1,
    ///         "applicantName": "John Doe",
    ///         "applicantEmail": "john.doe@example.com",
    ///         "applicantPhone": "+1234567890",
    ///         "linkedInProfile": "https://linkedin.com/in/johndoe",
    ///         "portfolioUrl": "https://johndoe.com/portfolio",
    ///         "coverLetterText": "I'm excited to apply for this position...",
    ///         "status": "Submitted",
    ///         "applicationDate": "2025-09-15T10:30:00Z",
    ///         "lastStatusChange": "2025-09-15T10:30:00Z",
    ///         "documents": [
    ///             {
    ///                 "jobApplicationId": 1,
    ///                 "documentType": "Resume",
    ///                 "originalFileName": "john_doe_resume.pdf",
    ///                 "mimeType": "application/pdf",
    ///                 "fileSize": 102400,
    ///                 "description": "John Doe's resume",
    ///                 "isRequired": true,
    ///                 "displayOrder": 1,
    ///                 "uploadDate": "2025-09-15T10:30:00Z",
    ///                 "gcsBucket": "maliev-career-applications",
    ///                 "gcsObjectName": "applications/000001/a1b2c3d4e5f6",
    ///                 "gcsUri": "gs://maliev-career-applications/applications/000001/a1b2c3d4e5f6"
    ///             }
    ///         ],
    ///         "notes": [
    ///             {
    ///                 "jobApplicationId": 1,
    ///                 "noteText": "Initial screening completed",
    ///                 "createdBy": "admin@example.com",
    ///                 "createdDate": "2025-09-15T11:00:00Z"
    ///             }
    ///         ]
    ///     }
    ///
    /// Sample response:
    ///
    ///     {
    ///         "id": 1,
    ///         "jobPositionId": 1,
    ///         "applicantName": "John Doe",
    ///         "applicantEmail": "john.doe@example.com",
    ///         "applicantPhone": "+1234567890",
    ///         "linkedInProfile": "https://linkedin.com/in/johndoe",
    ///         "portfolioUrl": "https://johndoe.com/portfolio",
    ///         "coverLetterText": "I'm excited to apply for this position...",
    ///         "status": "Submitted",
    ///         "applicationDate": "2025-09-15T10:30:00Z",
    ///         "lastStatusChange": "2025-09-15T10:30:00Z",
    ///         "documents": [
    ///             {
    ///                 "id": 1,
    ///                 "jobApplicationId": 1,
    ///                 "documentType": "Resume",
    ///                 "originalFileName": "john_doe_resume.pdf",
    ///                 "mimeType": "application/pdf",
    ///                 "fileSize": 102400,
    ///                 "description": "John Doe's resume",
    ///                 "isRequired": true,
    ///                 "displayOrder": 1,
    ///                 "uploadDate": "2025-09-15T10:30:00Z",
    ///                 "gcsBucket": "maliev-career-applications",
    ///                 "gcsObjectName": "applications/000001/a1b2c3d4e5f6",
    ///                 "gcsUri": "gs://maliev-career-applications/applications/000001/a1b2c3d4e5f6"
    ///             }
    ///         ],
    ///         "notes": [
    ///             {
    ///                 "id": 1,
    ///                 "jobApplicationId": 1,
    ///                 "noteText": "Initial screening completed",
    ///                 "createdBy": "admin@example.com",
    ///                 "createdDate": "2025-09-15T11:00:00Z"
    ///             }
    ///         ]
    ///     }
    ///
    /// Authentication:
    ///
    /// This endpoint requires authentication with a valid JWT token.
    ///
    /// Request body parameters:
    ///
    /// - jobPositionId: Required. ID of the job position to apply for
    /// - applicantName: Required. Applicant's full name (max 200 characters)
    /// - applicantEmail: Required. Applicant's email address (max 255 characters)
    /// - applicantPhone: Optional. Applicant's phone number (max 50 characters)
    /// - linkedInProfile: Optional. LinkedIn profile URL (max 500 characters)
    /// - portfolioUrl: Optional. Portfolio URL (max 500 characters)
    /// - coverLetterText: Optional. Cover letter text
    /// - status: Required. Application status (Submitted, UnderReview, Interviewed, Accepted, Rejected)
    /// - applicationDate: Required. Application date
    /// - lastStatusChange: Required. Last status change date
    /// - documents: Optional. List of application documents
    /// - notes: Optional. List of application notes
    ///
    /// Error responses:
    ///
    /// 400 Bad Request - When the request body is invalid or missing required fields
    /// 401 Unauthorized - When the request is not authenticated
    /// 500 Internal Server Error - When there is an unexpected error
    /// </remarks>
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<JobApplicationDto>> CreateJobApplication(
        [FromBody] CreateJobApplicationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var result = await _jobApplicationService.CreateAsync(request, cancellationToken);
            
            _logger.LogInformation("Job application created with ID {Id} for position {JobPositionId} by {ApplicantEmail}", 
                result.Id, result.JobPositionId, result.ApplicantEmail);
            
            return CreatedAtAction(
                nameof(GetJobApplication), 
                new { id = result.Id }, 
                result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating job application for position {JobPositionId}", request.JobPositionId);
            return StatusCode(500, "An error occurred while creating the job application");
        }
    }

    /// <summary>
    /// Updates an existing job application.
    /// </summary>
    /// <param name="id">The ID of the job application to update.</param>
    /// <param name="request">The request containing updated job application details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated job application, or NotFound if the application doesn't exist.</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     PUT careers/v1.0/applications/1
    ///     {
    ///         "jobPositionId": 1,
    ///         "applicantName": "John Smith",
    ///         "applicantEmail": "john.smith@example.com",
    ///         "applicantPhone": "+1234567890",
    ///         "linkedInProfile": "https://linkedin.com/in/johnsmith",
    ///         "portfolioUrl": "https://johnsmith.com/portfolio",
    ///         "coverLetterText": "I'm excited to update my application for this position...",
    ///         "status": "Interviewed",
    ///         "applicationDate": "2025-09-15T10:30:00Z",
    ///         "lastStatusChange": "2025-09-15T12:30:00Z",
    ///         "documents": [
    ///             {
    ///                 "jobApplicationId": 1,
    ///                 "documentType": "Resume",
    ///                 "originalFileName": "john_smith_resume.pdf",
    ///                 "mimeType": "application/pdf",
    ///                 "fileSize": 102400,
    ///                 "description": "John Smith's resume",
    ///                 "isRequired": true,
    ///                 "displayOrder": 1,
    ///                 "uploadDate": "2025-09-15T10:30:00Z",
    ///                 "gcsBucket": "maliev-career-applications",
    ///                 "gcsObjectName": "applications/000001/a1b2c3d4e5f6",
    ///                 "gcsUri": "gs://maliev-career-applications/applications/000001/a1b2c3d4e5f6"
    ///             }
    ///         ],
    ///         "notes": [
    ///             {
    ///                 "jobApplicationId": 1,
    ///                 "noteText": "Initial screening completed and candidate interviewed",
    ///                 "createdBy": "admin@example.com",
    ///                 "createdDate": "2025-09-15T12:00:00Z"
    ///             }
    ///         ]
    ///     }
    ///
    /// Sample response:
    ///
    ///     {
    ///         "id": 1,
    ///         "jobPositionId": 1,
    ///         "applicantName": "John Smith",
    ///         "applicantEmail": "john.smith@example.com",
    ///         "applicantPhone": "+1234567890",
    ///         "linkedInProfile": "https://linkedin.com/in/johnsmith",
    ///         "portfolioUrl": "https://johnsmith.com/portfolio",
    ///         "coverLetterText": "I'm excited to update my application for this position...",
    ///         "status": "Interviewed",
    ///         "applicationDate": "2025-09-15T10:30:00Z",
    ///         "lastStatusChange": "2025-09-15T12:30:00Z",
    ///         "documents": [
    ///             {
    ///                 "id": 1,
    ///                 "jobApplicationId": 1,
    ///                 "documentType": "Resume",
    ///                 "originalFileName": "john_smith_resume.pdf",
    ///                 "mimeType": "application/pdf",
    ///                 "fileSize": 102400,
    ///                 "description": "John Smith's resume",
    ///                 "isRequired": true,
    ///                 "displayOrder": 1,
    ///                 "uploadDate": "2025-09-15T10:30:00Z",
    ///                 "gcsBucket": "maliev-career-applications",
    ///                 "gcsObjectName": "applications/000001/a1b2c3d4e5f6",
    ///                 "gcsUri": "gs://maliev-career-applications/applications/000001/a1b2c3d4e5f6"
    ///             }
    ///         ],
    ///         "notes": [
    ///             {
    ///                 "id": 1,
    ///                 "jobApplicationId": 1,
    ///                 "noteText": "Initial screening completed and candidate interviewed",
    ///                 "createdBy": "admin@example.com",
    ///                 "createdDate": "2025-09-15T12:00:00Z"
    ///             }
    ///         ]
    ///     }
    ///
    /// Authentication:
    ///
    /// This endpoint requires authentication with a valid JWT token.
    ///
    /// Request body parameters:
    ///
    /// - jobPositionId: Required. ID of the job position to apply for
    /// - applicantName: Required. Applicant's full name (max 200 characters)
    /// - applicantEmail: Required. Applicant's email address (max 255 characters)
    /// - applicantPhone: Optional. Applicant's phone number (max 50 characters)
    /// - linkedInProfile: Optional. LinkedIn profile URL (max 500 characters)
    /// - portfolioUrl: Optional. Portfolio URL (max 500 characters)
    /// - coverLetterText: Optional. Cover letter text
    /// - status: Required. Application status (Submitted, UnderReview, Interviewed, Accepted, Rejected)
    /// - applicationDate: Required. Application date
    /// - lastStatusChange: Required. Last status change date
    /// - documents: Optional. List of application documents
    /// - notes: Optional. List of application notes
    ///
    /// Error responses:
    ///
    /// 400 Bad Request - When the request body is invalid or missing required fields
    /// 401 Unauthorized - When the request is not authenticated
    /// 404 Not Found - When the job application with the specified ID does not exist
    /// 500 Internal Server Error - When there is an unexpected error
    /// </remarks>
    [HttpPut("{id:int}")]
    [Authorize]
    public async Task<ActionResult<JobApplicationDto>> UpdateJobApplication(
        int id,
        [FromBody] UpdateJobApplicationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var result = await _jobApplicationService.UpdateStatusAsync(id, request, cancellationToken);
            
            if (result == null)
            {
                return NotFound($"Job application with ID {id} not found");
            }

            _logger.LogInformation("Job application {Id} status updated to {Status} by user", id, request.Status);
            
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating job application status {Id}", id);
            return StatusCode(500, "An error occurred while updating the job application status");
        }
    }

    /// <summary>
    /// Deletes a job application (marks it as inactive).
    /// </summary>
    /// <param name="id">The ID of the job application to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>NoContent if successful, or NotFound if the application doesn't exist.</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     DELETE careers/v1.0/applications/1
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
    /// 404 Not Found - When the job application with the specified ID does not exist
    /// 500 Internal Server Error - When there is an unexpected error
    /// </remarks>
    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<ActionResult> DeleteJobApplication(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var success = await _jobApplicationService.DeleteAsync(id, cancellationToken);
            
            if (!success)
            {
                return NotFound($"Job application with ID {id} not found");
            }

            _logger.LogInformation("Job application {Id} deleted by user", id);
            
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting job application {Id}", id);
            return StatusCode(500, "An error occurred while deleting the job application");
        }
    }

    /// <summary>
    /// Checks if a job application with the specified ID exists.
    /// </summary>
    /// <param name="id">The ID of the job application to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the job application exists, false otherwise.</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET careers/v1.0/applications/1/exists
    ///
    /// Sample response:
    ///
    ///     true
    ///
    /// Error responses:
    ///
    /// 500 Internal Server Error - When there is an unexpected error
    /// </remarks>
    [HttpGet("{id:int}/exists")]
    [Authorize]
    public async Task<ActionResult<bool>> CheckJobApplicationExists(int id, CancellationToken cancellationToken = default)
    {
        var exists = await _jobApplicationService.ExistsAsync(id, cancellationToken);
        return Ok(exists);
    }

        /// <summary>
    /// Validates if a job application with the specified applicant email and job position ID already exists.
    /// </summary>
    /// <param name="email">The applicant email to validate.</param>
    /// <param name="jobPositionId">The job position ID to validate.</param>
    /// <param name="excludeId">Optional ID to exclude from validation (for updates).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the job application is valid (doesn't exist), false otherwise.</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET careers/v1.0/applications/validate?email=john.doe@example.com&amp;jobPositionId=1
    ///
    /// Sample response:
    ///
    ///     true
    ///
    /// Query parameters:
    ///
    /// - email: Required. Applicant email to validate
    /// - jobPositionId: Required. Job position ID to validate
    /// - excludeId: Optional. ID to exclude from validation (for updates)
    ///
    /// Error responses:
    ///
    /// 400 Bad Request - When email or jobPositionId is missing or invalid
    /// 500 Internal Server Error - When there is an unexpected error
    /// </remarks>
    [HttpGet("validate")]
    [Authorize]
    public async Task<ActionResult<bool>> ValidateJobApplication(
        [FromQuery] string email,
        [FromQuery] int jobPositionId,
        [FromQuery] int? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email) || jobPositionId <= 0)
        {
            return BadRequest("Email and valid job position ID are required");
        }

        var hasExisting = await _jobApplicationService.HasExistingApplicationAsync(email, jobPositionId, cancellationToken);
        return Ok(hasExisting);
    }

    [HttpGet("statuses")]
    [AllowAnonymous]
    public ActionResult<IEnumerable<string>> GetValidStatuses()
    {
        return Ok(ApplicationStatus.ValidStatuses);
    }
}