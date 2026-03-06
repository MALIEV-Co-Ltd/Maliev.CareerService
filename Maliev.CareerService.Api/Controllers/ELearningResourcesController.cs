using Asp.Versioning;
using Maliev.Aspire.ServiceDefaults.Authorization;
using Maliev.CareerService.Api.Authentication;
using Maliev.CareerService.Application.Models.ELearningResources;
using Maliev.CareerService.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Maliev.CareerService.Api.Controllers;

/// <summary>
/// Controller for managing e-learning resources
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("career/v{version:apiVersion}/elearning-resources")]
[Produces("application/json")]
public class ELearningResourcesController(
    IELearningResourceService eLearningResourceService,
    ILogger<ELearningResourcesController> logger) : ControllerBase
{
    private readonly IELearningResourceService _eLearningResourceService = eLearningResourceService;
    private readonly ILogger<ELearningResourcesController> _logger = logger;

    /// <summary>
    /// Gets active e-learning resources with optional filters
    /// </summary>
    /// <param name="category">Filter by category</param>
    /// <param name="resourceType">Filter by resource type (Video, Document, Interactive, Quiz)</param>
    /// <param name="offset">Number of items to skip (default: 0)</param>
    /// <param name="limit">Number of items to return (default: 20, max: 100)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of e-learning resources</returns>
    [HttpGet]
    [RequirePermission(CareerPermissions.Trainings.Read)]
    [ResponseCache(Duration = 300, VaryByHeader = "Authorization", VaryByQueryKeys = new[] { "category", "resourceType", "offset", "limit" })]
    [ProducesResponseType(typeof(ELearningResourceListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ELearningResourceListResponse>> GetELearningResources(
        [FromQuery] string? category = null,
        [FromQuery] string? resourceType = null,
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

        ELearningResourceListResponse result;

        if (!string.IsNullOrWhiteSpace(category) || !string.IsNullOrWhiteSpace(resourceType))
        {
            // Use filter method
            result = await _eLearningResourceService.FilterResourcesAsync(
                category,
                resourceType,
                pageNumber,
                pageSize,
                cancellationToken);
        }
        else
        {
            // Get all active resources
            result = await _eLearningResourceService.GetActiveResourcesAsync(
                pageNumber,
                pageSize,
                cancellationToken);
        }

        return Ok(result);
    }

    /// <summary>
    /// Gets a specific e-learning resource by ID
    /// </summary>
    /// <param name="id">E-learning resource ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>E-learning resource details</returns>
    [HttpGet("{id:guid}")]
    [RequirePermission(CareerPermissions.Trainings.Read)]
    [ResponseCache(Duration = 600, VaryByHeader = "Authorization")]
    [ProducesResponseType(typeof(ELearningResourceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ELearningResourceResponse>> GetELearningResource(
        Guid id,
        CancellationToken cancellationToken)
    {
        var resource = await _eLearningResourceService.GetResourceByIdAsync(id, cancellationToken);

        if (resource == null)
        {
            return NotFound(new { error = $"E-learning resource {id} not found" });
        }

        return Ok(resource);
    }
}
