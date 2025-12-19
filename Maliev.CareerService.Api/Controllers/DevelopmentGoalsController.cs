using Asp.Versioning;
using Maliev.CareerService.Api.Models.DevelopmentGoals;
using Maliev.CareerService.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Maliev.CareerService.Api.Controllers;

/// <summary>
/// Controller for managing Development Goals
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("career/v{version:apiVersion}")]
[Produces("application/json")]
[Authorize(Roles = "Employee")]
public class DevelopmentGoalsController(
    IDevelopmentGoalService developmentGoalService,
    ILogger<DevelopmentGoalsController> logger) : ControllerBase
{
    private readonly IDevelopmentGoalService _developmentGoalService = developmentGoalService;
    private readonly ILogger<DevelopmentGoalsController> _logger = logger;

    /// <summary>
    /// Creates a new development goal for an IDP
    /// </summary>
    [HttpPost("idps/{idpId:guid}/goals")]
    [ProducesResponseType(typeof(DevelopmentGoalResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<DevelopmentGoalResponse>> CreateGoal(
        Guid idpId,
        [FromBody] CreateDevelopmentGoalRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var employeeId = GetAuthenticatedUserId();
            if (employeeId == Guid.Empty)
            {
                return Unauthorized(new { error = "User ID not found in claims" });
            }

            var result = await _developmentGoalService.CreateGoalAsync(
                idpId,
                request,
                employeeId,
                cancellationToken);

            return CreatedAtAction(
                nameof(GetGoal),
                new { id = result.Id },
                result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized goal creation attempt");
            return Forbid();
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            _logger.LogWarning(ex, "IDP {IdpId} not found", idpId);
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets a specific development goal (for testing/verification)
    /// </summary>
    [HttpGet("goals/{id:guid}")]
    [ProducesResponseType(typeof(DevelopmentGoalResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<ActionResult<DevelopmentGoalResponse>> GetGoal(
        Guid id,
        CancellationToken cancellationToken)
    {
        // This endpoint is primarily for CreatedAtAction redirects
        // In production, goals are retrieved via the IDP endpoint
        return Task.FromResult<ActionResult<DevelopmentGoalResponse>>(Ok(new DevelopmentGoalResponse { Id = id }));
    }

    /// <summary>
    /// Updates a development goal
    /// </summary>
    [HttpPut("goals/{id:guid}")]
    [ProducesResponseType(typeof(DevelopmentGoalResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<DevelopmentGoalResponse>> UpdateGoal(
        Guid id,
        [FromBody] UpdateDevelopmentGoalRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var employeeId = GetAuthenticatedUserId();
            if (employeeId == Guid.Empty)
            {
                return Unauthorized(new { error = "User ID not found in claims" });
            }

            var result = await _developmentGoalService.UpdateGoalAsync(
                id,
                request,
                employeeId,
                cancellationToken);

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized goal update attempt");
            return Forbid();
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            _logger.LogWarning(ex, "Goal {GoalId} not found", id);
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid goal update attempt");
            return BadRequest(new { error = ex.Message });
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict updating goal {GoalId}", id);
            return Conflict(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Updates the status of a development goal
    /// </summary>
    [HttpPatch("goals/{id:guid}/status")]
    [ProducesResponseType(typeof(DevelopmentGoalResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<DevelopmentGoalResponse>> UpdateGoalStatus(
        Guid id,
        [FromBody] UpdateGoalStatusRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var employeeId = GetAuthenticatedUserId();
            if (employeeId == Guid.Empty)
            {
                return Unauthorized(new { error = "User ID not found in claims" });
            }

            var result = await _developmentGoalService.UpdateGoalStatusAsync(
                id,
                request,
                employeeId,
                cancellationToken);

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized goal status update attempt");
            return Forbid();
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            _logger.LogWarning(ex, "Goal {GoalId} not found", id);
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid goal status update attempt");
            return BadRequest(new { error = ex.Message });
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict updating goal status {GoalId}", id);
            return Conflict(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets the authenticated user's ID from JWT claims
    /// </summary>
    private Guid GetAuthenticatedUserId()
    {
        var userIdClaim = User.FindFirst("sub")?.Value ??
                         User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
}
