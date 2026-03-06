using Asp.Versioning;
using Maliev.Aspire.ServiceDefaults.Authorization;
using Maliev.CareerService.Api.Authentication;
using Maliev.CareerService.Application.Models.Enrollments;
using Maliev.CareerService.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Maliev.CareerService.Api.Controllers;

/// <summary>
/// Controller for managing training enrollments
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("career/v{version:apiVersion}/training-enrollments")]
[Produces("application/json")]
public class EnrollmentsController(
    IEnrollmentService enrollmentService,
    ILogger<EnrollmentsController> logger) : ControllerBase
{
    private readonly IEnrollmentService _enrollmentService = enrollmentService;
    private readonly ILogger<EnrollmentsController> _logger = logger;

    /// <summary>
    /// Gets enrollments for the authenticated employee with optional status filter
    /// </summary>
    /// <param name="status">Optional status filter (Enrolled, InProgress, Completed, Cancelled)</param>
    /// <param name="offset">Number of items to skip (default: 0)</param>
    /// <param name="limit">Number of items to return (default: 20, max: 100)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of enrollments</returns>
    [HttpGet]
    [RequirePermission(CareerPermissions.Trainings.Read)]
    [ProducesResponseType(typeof(TrainingEnrollmentListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<TrainingEnrollmentListResponse>> GetEnrollments(
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

        // Get authenticated employee ID from JWT claims
        var employeeId = GetAuthenticatedUserId();

        // Calculate page number from offset
        var pageNumber = (offset / limit) + 1;
        var pageSize = limit;

        var result = await _enrollmentService.GetEmployeeEnrollmentsAsync(
            employeeId,
            status,
            pageNumber,
            pageSize,
            cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Enrolls the authenticated employee in a training program
    /// </summary>
    /// <param name="request">Enrollment request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created enrollment</returns>
    [HttpPost]
    [RequirePermission(CareerPermissions.Trainings.Enroll)]
    [ProducesResponseType(typeof(TrainingEnrollmentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<TrainingEnrollmentResponse>> EnrollInTraining(
        [FromBody] EnrollInTrainingRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Get authenticated employee ID from JWT claims
            var employeeId = GetAuthenticatedUserId();

            var result = await _enrollmentService.EnrollEmployeeAsync(request, employeeId, cancellationToken);

            return CreatedAtAction(
                nameof(GetEnrollments),
                new { },
                result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to enroll employee due to validation error");

            // Check specific error types
            if (ex.Message.Contains("not found"))
            {
                return NotFound(new { error = ex.Message });
            }

            if (ex.Message.Contains("already enrolled"))
            {
                return Conflict(new { error = ex.Message });
            }

            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Marks a training enrollment as completed (HR staff only)
    /// </summary>
    /// <param name="id">Enrollment ID</param>
    /// <param name="request">Completion request with notes and row version</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated enrollment</returns>
    [HttpPatch("{id:guid}/complete")]
    [RequirePermission(CareerPermissions.Trainings.Complete)]
    [ProducesResponseType(typeof(TrainingEnrollmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<TrainingEnrollmentResponse>> MarkTrainingComplete(
        Guid id,
        [FromBody] MarkTrainingCompleteRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Get authenticated HR staff user ID from JWT claims
            var userId = GetAuthenticatedUserId();

            var result = await _enrollmentService.MarkCompletedAsync(id, request, userId, cancellationToken);

            if (result == null)
            {
                return NotFound(new { error = $"Enrollment {id} not found" });
            }

            return Ok(result);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict when marking enrollment {Id} as completed", id);
            return Conflict(new { error = "The enrollment has been modified by another user. Please refresh and try again." });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to mark enrollment {Id} as completed due to validation error", id);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets the authenticated user ID from JWT claims
    /// </summary>
    /// <returns>User ID</returns>
    private Guid GetAuthenticatedUserId()
    {
        // Extract user ID from JWT claims
        var userIdClaim = User.FindFirst("sub") ?? User.FindFirst("user_id");
        if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return userId;
        }

        // Fallback for development/testing
        return Guid.Empty;
    }
}
