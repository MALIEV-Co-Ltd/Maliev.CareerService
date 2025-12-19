using Asp.Versioning;
using Maliev.CareerService.Api.Models.Applications;
using Maliev.CareerService.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maliev.CareerService.Api.Controllers;

/// <summary>
/// Controller for managing job applications
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("career/v{version:apiVersion}/job-applications")]
[Produces("application/json")]
public class ApplicationsController(
    IApplicationService applicationService,
    ILogger<ApplicationsController> logger) : ControllerBase
{
    private readonly IApplicationService _applicationService = applicationService;
    private readonly ILogger<ApplicationsController> _logger = logger;

    /// <summary>
    /// Submits a new job application
    /// </summary>
    /// <param name="request">Job application submission request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created job application</returns>
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(typeof(JobApplicationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<JobApplicationResponse>> SubmitApplication(
        [FromBody] SubmitJobApplicationRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _applicationService.SubmitApplicationAsync(request, cancellationToken);

            return CreatedAtAction(
                nameof(GetApplication),
                new { id = result.Id },
                result);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            _logger.LogWarning(ex, "Job posting not found: {Message}", ex.Message);
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("deadline") || ex.Message.Contains("duplicate"))
        {
            _logger.LogWarning(ex, "Application submission rejected: {Message}", ex.Message);
            return Conflict(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid file ID in application submission");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets job applications with filtering
    /// </summary>
    /// <param name="jobPostingId">Filter by job posting ID</param>
    /// <param name="status">Filter by application status</param>
    /// <param name="offset">Number of items to skip (default: 0)</param>
    /// <param name="limit">Number of items to return (default: 20, max: 100)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of job applications</returns>
    [HttpGet]
    [Authorize(Roles = "Applicant,HRStaff")]
    [ProducesResponseType(typeof(JobApplicationListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<JobApplicationListResponse>> GetApplications(
        [FromQuery] Guid? jobPostingId = null,
        [FromQuery] string? status = null,
        [FromQuery] int offset = 0,
        [FromQuery] int limit = 20,
        CancellationToken cancellationToken = default)
    {
        // Validate limit
        if (limit <= 0 || limit > 100)
        {
            return BadRequest(new { error = "Limit must be between 1 and 100" });
        }

        // Calculate page number from offset
        var pageNumber = (offset / limit) + 1;
        var pageSize = limit;

        // Check user role
        var isHRStaff = User.IsInRole("HRStaff");

        if (!isHRStaff)
        {
            // Applicants can only see their own applications
            var userEmail = GetAuthenticatedUserEmail();
            if (string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized(new { error = "User email not found in claims" });
            }

            var result = await _applicationService.GetApplicantApplicationsAsync(
                userEmail,
                pageNumber,
                pageSize,
                cancellationToken);

            return Ok(result);
        }
        else
        {
            // HR Staff can see all applications
            // TODO: Implement GetAllApplicationsAsync in ApplicationService with filters
            // For now, return not implemented
            return StatusCode(StatusCodes.Status501NotImplemented,
                new { error = "Filtering all applications by HR staff not yet implemented" });
        }
    }

    /// <summary>
    /// Gets a specific job application by ID
    /// </summary>
    /// <param name="id">Job application ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Job application details</returns>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Applicant,HRStaff")]
    [ProducesResponseType(typeof(JobApplicationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<JobApplicationResponse>> GetApplication(
        Guid id,
        CancellationToken cancellationToken)
    {
        var application = await _applicationService.GetApplicationByIdAsync(id, cancellationToken);

        if (application == null)
        {
            return NotFound(new { error = $"Job application {id} not found" });
        }

        // Check access: Applicants can only view their own applications
        if (!User.IsInRole("HRStaff"))
        {
            var userEmail = GetAuthenticatedUserEmail();
            if (string.IsNullOrEmpty(userEmail) ||
                !application.ApplicantEmail.Equals(userEmail, StringComparison.OrdinalIgnoreCase))
            {
                return Forbid();
            }
        }

        return Ok(application);
    }

    /// <summary>
    /// Updates the status of a job application
    /// </summary>
    /// <param name="id">Job application ID</param>
    /// <param name="request">Status update request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated job application</returns>
    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "HRStaff")]
    [ProducesResponseType(typeof(JobApplicationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<JobApplicationResponse>> UpdateApplicationStatus(
        Guid id,
        [FromBody] UpdateApplicationStatusRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var hrUserId = GetAuthenticatedUserId();
            if (hrUserId == Guid.Empty)
            {
                return Unauthorized(new { error = "User ID not found in claims" });
            }

            var result = await _applicationService.UpdateApplicationStatusAsync(
                id,
                request,
                hrUserId,
                cancellationToken);

            return Ok(result);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            _logger.LogWarning(ex, "Application {ApplicationId} not found", id);
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Invalid status transition"))
        {
            _logger.LogWarning(ex, "Invalid status transition for application {ApplicationId}", id);
            return BadRequest(new { error = ex.Message });
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict updating application {ApplicationId}", id);
            return Conflict(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets the status change history for a job application
    /// </summary>
    /// <param name="id">Job application ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Status change history</returns>
    [HttpGet("{id:guid}/status-history")]
    [Authorize(Roles = "HRStaff")]
    [ProducesResponseType(typeof(StatusHistoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<StatusHistoryResponse>> GetStatusHistory(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _applicationService.GetStatusHistoryAsync(id, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            _logger.LogWarning(ex, "Application {ApplicationId} not found", id);
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets the authenticated user's email from JWT claims
    /// </summary>
    /// <returns>User email</returns>
    private string? GetAuthenticatedUserEmail()
    {
        return User.FindFirst("email")?.Value ??
               User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value;
    }

    /// <summary>
    /// Gets the authenticated user's ID from JWT claims
    /// </summary>
    /// <returns>User ID</returns>
    private Guid GetAuthenticatedUserId()
    {
        var userIdClaim = User.FindFirst("sub")?.Value ??
                         User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
}
