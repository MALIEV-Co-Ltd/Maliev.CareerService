using Asp.Versioning;
using Maliev.Aspire.ServiceDefaults.Authorization;
using Maliev.CareerService.Api.Authentication;
using Maliev.CareerService.Api.Models.JobPostings;
using Maliev.CareerService.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace Maliev.CareerService.Api.Controllers;

/// <summary>
/// Controller for managing job postings
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("career/v{version:apiVersion}/job-postings")]
[Produces("application/json")]
public class JobPostingsController(
    IJobPostingService jobPostingService,
    ILogger<JobPostingsController> logger) : ControllerBase
{
    private readonly IJobPostingService _jobPostingService = jobPostingService;
    private readonly ILogger<JobPostingsController> _logger = logger;

    /// <summary>
    /// Gets active job postings with optional filters
    /// </summary>
    /// <param name="department">Filter by department</param>
    /// <param name="location">Filter by location</param>
    /// <param name="employmentType">Filter by employment type (Full-time, Part-time, Contract, Internship)</param>
    /// <param name="search">Search keyword for position title, description, requirements, or responsibilities</param>
    /// <param name="offset">Number of items to skip (default: 0)</param>
    /// <param name="limit">Number of items to return (default: 20, max: 100)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of job postings</returns>
    [HttpGet]
    [AllowAnonymous]
    [EnableRateLimiting("anonymous")]
    [ResponseCache(Duration = 60, VaryByQueryKeys = new[] { "department", "location", "employmentType", "search", "offset", "limit" })]
    [ProducesResponseType(typeof(JobPostingListResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<JobPostingListResponse>> GetJobPostings(
        [FromQuery] string? department = null,
        [FromQuery] string? location = null,
        [FromQuery] string? employmentType = null,
        [FromQuery] string? search = null,
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

        JobPostingListResponse result;

        if (!string.IsNullOrWhiteSpace(search) ||
            !string.IsNullOrWhiteSpace(department) ||
            !string.IsNullOrWhiteSpace(location) ||
            !string.IsNullOrWhiteSpace(employmentType))
        {
            // Use search method with filters
            result = await _jobPostingService.SearchPostingsAsync(
                search,
                department,
                location,
                employmentType,
                pageNumber,
                pageSize,
                cancellationToken);
        }
        else
        {
            // Get all active postings
            result = await _jobPostingService.GetActivePostingsAsync(
                pageNumber,
                pageSize,
                cancellationToken);
        }

        return Ok(result);
    }

    /// <summary>
    /// Gets a specific job posting by ID
    /// </summary>
    /// <param name="id">Job posting ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Job posting details</returns>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [EnableRateLimiting("anonymous")]
    [ResponseCache(Duration = 120, VaryByQueryKeys = new[] { "id" })]
    [ProducesResponseType(typeof(JobPostingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<JobPostingResponse>> GetJobPosting(
        Guid id,
        CancellationToken cancellationToken)
    {
        var posting = await _jobPostingService.GetPostingByIdAsync(id, cancellationToken);

        if (posting == null)
        {
            return NotFound(new { error = $"Job posting {id} not found" });
        }

        return Ok(posting);
    }

    /// <summary>
    /// Creates a new job posting
    /// </summary>
    /// <param name="request">Job posting creation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created job posting</returns>
    [HttpPost]
    [RequirePermission(CareerPermissions.JobPostings.Manage)]
    [ProducesResponseType(typeof(JobPostingResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<JobPostingResponse>> CreateJobPosting(
        [FromBody] CreateJobPostingRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Get authenticated user ID (would come from JWT claims in real implementation)
            var userId = GetAuthenticatedUserId();

            var result = await _jobPostingService.CreatePostingAsync(request, userId, cancellationToken);

            return CreatedAtAction(
                nameof(GetJobPosting),
                new { id = result.Id },
                result);
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message.Contains("not found"))
            {
                return NotFound(new { error = ex.Message });
            }

            _logger.LogWarning(ex, "Failed to create job posting due to validation error");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Updates an existing job posting
    /// </summary>
    /// <param name="id">Job posting ID</param>
    /// <param name="request">Job posting update request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated job posting</returns>
    [HttpPut("{id:guid}")]
    [RequirePermission(CareerPermissions.JobPostings.Manage)]
    [ProducesResponseType(typeof(JobPostingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<JobPostingResponse>> UpdateJobPosting(
        Guid id,
        [FromBody] UpdateJobPostingRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Get authenticated user ID
            var userId = GetAuthenticatedUserId();

            var result = await _jobPostingService.UpdatePostingAsync(id, request, userId, cancellationToken);

            if (result == null)
            {
                return NotFound(new { error = $"Job posting {id} not found" });
            }

            return Ok(result);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict when updating job posting {Id}", id);
            return Conflict(new { error = "The job posting has been modified by another user. Please refresh and try again." });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to update job posting {Id} due to validation error", id);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Deletes a job posting (soft delete)
    /// </summary>
    /// <param name="id">Job posting ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content</returns>
    [HttpDelete("{id:guid}")]
    [RequirePermission(CareerPermissions.JobPostings.Manage)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteJobPosting(
        Guid id,
        CancellationToken cancellationToken)
    {
        // Get authenticated user ID
        var userId = GetAuthenticatedUserId();

        var success = await _jobPostingService.DeletePostingAsync(id, userId, cancellationToken);

        if (!success)
        {
            return NotFound(new { error = $"Job posting {id} not found" });
        }

        return NoContent();
    }

    /// <summary>
    /// Gets the authenticated user ID from JWT claims
    /// </summary>
    /// <returns>User ID</returns>
    private Guid GetAuthenticatedUserId()
    {
        // In a real implementation, this would extract the user ID from JWT claims
        // For now, return a placeholder
        var userIdClaim = User.FindFirst("sub") ?? User.FindFirst("user_id");
        if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return userId;
        }

        // Fallback for development/testing
        return Guid.Empty;
    }
}
