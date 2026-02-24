using Asp.Versioning;
using Maliev.Aspire.ServiceDefaults.Authorization;
using Maliev.CareerService.Api.Authentication;
using Maliev.CareerService.Api.Models.TrainingRecords;
using Maliev.CareerService.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Maliev.CareerService.Api.Controllers;

/// <summary>
/// Controller for managing mandatory training requirements (Feature 003)
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("career/v{version:apiVersion}/mandatory-training")]
[Produces("application/json")]
public class MandatoryTrainingController(
    IMandatoryTrainingService mandatoryTrainingService,
    ILogger<MandatoryTrainingController> logger) : ControllerBase
{
    private readonly IMandatoryTrainingService _mandatoryTrainingService = mandatoryTrainingService;
    private readonly ILogger<MandatoryTrainingController> _logger = logger;

    /// <summary>
    /// Creates a new mandatory training requirement
    /// </summary>
    [HttpPost]
    [RequirePermission(CareerPermissions.MandatoryTrainings.Manage)]
    [ProducesResponseType(typeof(MandatoryTrainingRequirementDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<MandatoryTrainingRequirementDto>> CreateRequirement(
        [FromBody] CreateMandatoryRequirementRequest request,
        CancellationToken cancellationToken)
    {
        var currentUserId = GetAuthenticatedUserId();
        if (currentUserId == Guid.Empty)
        {
            return Unauthorized(new { error = "User ID not found in claims" });
        }

        try
        {
            var result = await _mandatoryTrainingService.CreateRequirementAsync(
                request,
                currentUserId,
                cancellationToken);

            return CreatedAtAction(
                nameof(GetRequirement),
                new { id = result.Id },
                result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Validation failed for mandatory requirement: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets all mandatory training requirements
    /// </summary>
    [HttpGet]
    [RequirePermission(CareerPermissions.MandatoryTrainings.View)]
    [ProducesResponseType(typeof(List<MandatoryTrainingRequirementDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<MandatoryTrainingRequirementDto>>> GetRequirements(
        [FromQuery] bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        var result = await _mandatoryTrainingService.GetAllRequirementsAsync(activeOnly, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets a specific mandatory training requirement by ID
    /// </summary>
    [HttpGet("{id:guid}", Name = nameof(GetRequirement))]
    [RequirePermission(CareerPermissions.MandatoryTrainings.View)]
    [ProducesResponseType(typeof(MandatoryTrainingRequirementDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<MandatoryTrainingRequirementDto>> GetRequirement(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mandatoryTrainingService.GetByIdAsync(id, cancellationToken);

        if (result == null)
        {
            return NotFound(new { error = "Requirement not found" });
        }

        return Ok(result);
    }

    /// <summary>
    /// Updates an existing mandatory training requirement
    /// </summary>
    [HttpPut("{id:guid}")]
    [RequirePermission(CareerPermissions.MandatoryTrainings.Manage)]
    [ProducesResponseType(typeof(MandatoryTrainingRequirementDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<MandatoryTrainingRequirementDto>> UpdateRequirement(
        Guid id,
        [FromBody] UpdateMandatoryRequirementRequest request,
        CancellationToken cancellationToken)
    {
        var currentUserId = GetAuthenticatedUserId();
        if (currentUserId == Guid.Empty)
        {
            return Unauthorized(new { error = "User ID not found in claims" });
        }

        var result = await _mandatoryTrainingService.UpdateRequirementAsync(id, request, currentUserId, cancellationToken);

        if (result == null)
        {
            return NotFound(new { error = "Requirement not found" });
        }

        return Ok(result);
    }

    /// <summary>
    /// Deactivates a mandatory training requirement
    /// </summary>
    [HttpDelete("{id:guid}")]
    [RequirePermission(CareerPermissions.MandatoryTrainings.Manage)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeactivateRequirement(
        Guid id,
        CancellationToken cancellationToken)
    {
        var currentUserId = GetAuthenticatedUserId();
        if (currentUserId == Guid.Empty)
        {
            return Unauthorized(new { error = "User ID not found in claims" });
        }

        var success = await _mandatoryTrainingService.DeactivateRequirementAsync(id, currentUserId, cancellationToken);

        if (!success)
        {
            return NotFound(new { error = "Requirement not found" });
        }

        return NoContent();
    }

    /// <summary>
    /// Extracts authenticated user ID from JWT claims
    /// </summary>
    private Guid GetAuthenticatedUserId()
    {
        var userIdClaim = User.FindFirst("sub")?.Value ??
                         User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
}
