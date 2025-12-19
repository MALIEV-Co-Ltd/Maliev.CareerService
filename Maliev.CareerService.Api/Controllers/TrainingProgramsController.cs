using Asp.Versioning;
using Maliev.CareerService.Api.Models.TrainingPrograms;
using Maliev.CareerService.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Maliev.CareerService.Api.Controllers;

/// <summary>
/// Controller for managing training programs
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("career/v{version:apiVersion}/training-programs")]
[Produces("application/json")]
public class TrainingProgramsController(
    ITrainingProgramService trainingProgramService,
    ILogger<TrainingProgramsController> logger) : ControllerBase
{
    private readonly ITrainingProgramService _trainingProgramService = trainingProgramService;
    private readonly ILogger<TrainingProgramsController> _logger = logger;

    /// <summary>
    /// Gets active training programs with optional filters
    /// </summary>
    /// <param name="category">Filter by category</param>
    /// <param name="isMandatory">Filter by mandatory status</param>
    /// <param name="offset">Number of items to skip (default: 0)</param>
    /// <param name="limit">Number of items to return (default: 20, max: 100)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of training programs</returns>
    [HttpGet]
    [Authorize(Roles = "Employee,HRStaff")]
    [ResponseCache(Duration = 300, VaryByHeader = "Authorization", VaryByQueryKeys = new[] { "category", "isMandatory", "offset", "limit" })]
    [ProducesResponseType(typeof(TrainingProgramListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<TrainingProgramListResponse>> GetTrainingPrograms(
        [FromQuery] string? category = null,
        [FromQuery] bool? isMandatory = null,
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

        TrainingProgramListResponse result;

        if (!string.IsNullOrWhiteSpace(category) || isMandatory.HasValue)
        {
            // Use filter method
            result = await _trainingProgramService.FilterProgramsAsync(
                category,
                isMandatory,
                pageNumber,
                pageSize,
                cancellationToken);
        }
        else
        {
            // Get all active programs
            result = await _trainingProgramService.GetActiveProgramsAsync(
                pageNumber,
                pageSize,
                cancellationToken);
        }

        return Ok(result);
    }

    /// <summary>
    /// Gets a specific training program by ID
    /// </summary>
    /// <param name="id">Training program ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Training program details</returns>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Employee,HRStaff")]
    [ResponseCache(Duration = 600, VaryByHeader = "Authorization")]
    [ProducesResponseType(typeof(TrainingProgramResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<TrainingProgramResponse>> GetTrainingProgram(
        Guid id,
        CancellationToken cancellationToken)
    {
        var program = await _trainingProgramService.GetProgramByIdAsync(id, cancellationToken);

        if (program == null)
        {
            return NotFound(new { error = $"Training program {id} not found" });
        }

        return Ok(program);
    }

    /// <summary>
    /// Creates a new training program
    /// </summary>
    /// <param name="request">Training program creation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created training program</returns>
    [HttpPost]
    [Authorize(Roles = "HRStaff")]
    [ProducesResponseType(typeof(TrainingProgramResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<TrainingProgramResponse>> CreateTrainingProgram(
        [FromBody] CreateTrainingProgramRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Get authenticated user ID from JWT claims
            var userId = GetAuthenticatedUserId();

            var result = await _trainingProgramService.CreateProgramAsync(request, userId, cancellationToken);

            return CreatedAtAction(
                nameof(GetTrainingProgram),
                new { id = result.Id },
                result);
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message.Contains("not found"))
            {
                return NotFound(new { error = ex.Message });
            }

            _logger.LogWarning(ex, "Failed to create training program due to validation error");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Updates an existing training program
    /// </summary>
    /// <param name="id">Training program ID</param>
    /// <param name="request">Training program update request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated training program</returns>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "HRStaff")]
    [ProducesResponseType(typeof(TrainingProgramResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<TrainingProgramResponse>> UpdateTrainingProgram(
        Guid id,
        [FromBody] UpdateTrainingProgramRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Get authenticated user ID
            var userId = GetAuthenticatedUserId();

            var result = await _trainingProgramService.UpdateProgramAsync(id, request, userId, cancellationToken);

            if (result == null)
            {
                return NotFound(new { error = $"Training program {id} not found" });
            }

            return Ok(result);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict when updating training program {Id}", id);
            return Conflict(new { error = "The training program has been modified by another user. Please refresh and try again." });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to update training program {Id} due to validation error", id);
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
