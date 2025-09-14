using Asp.Versioning;
using Maliev.CareerService.Api.Models;
using Maliev.CareerService.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Maliev.CareerService.Api.Controllers;

/// <summary>
/// Controller for managing job positions in the career service.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("careers/v{version:apiVersion}/positions")]
[EnableRateLimiting("CareerPolicy")]
public class JobPositionController : ControllerBase
{
    private readonly IJobPositionService _jobPositionService;
    private readonly ILogger<JobPositionController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="JobPositionController"/> class.
    /// </summary>
    /// <param name="jobPositionService">The job position service.</param>
    /// <param name="logger">The logger.</param>
    public JobPositionController(
        IJobPositionService jobPositionService,
        ILogger<JobPositionController> logger)
    {
        _jobPositionService = jobPositionService;
        _logger = logger;
    }

    /// <summary>
    /// Gets a job position by its ID.
    /// </summary>
    /// <param name="id">The ID of the job position to retrieve.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The job position with the specified ID, or NotFound if not found.</returns>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<JobPositionDto>> GetJobPosition(int id, CancellationToken cancellationToken = default)
    {
        var position = await _jobPositionService.GetByIdAsync(id, cancellationToken);
        
        if (position == null)
        {
            return NotFound($"Job position with ID {id} not found");
        }

        return Ok(position);
    }

    /// <summary>
    /// Searches for job positions based on the provided search criteria.
    /// </summary>
    /// <param name="request">The search request containing filter criteria.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paged result of job positions matching the search criteria.</returns>
    [HttpGet("search")]
    [AllowAnonymous]
    public async Task<ActionResult<PagedResult<JobPositionDto>>> SearchJobPositions(
        [FromQuery] JobPositionSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _jobPositionService.SearchAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets publicly available job positions based on the provided search criteria.
    /// </summary>
    /// <param name="request">The search request containing filter criteria.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paged result of public job positions matching the search criteria.</returns>
    [HttpGet("public")]
    [AllowAnonymous]
    public async Task<ActionResult<PagedResult<JobPositionDto>>> GetPublicJobPositions(
        [FromQuery] JobPositionSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _jobPositionService.GetPublicPositionsAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets a list of unique departments from all job positions.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of department names.</returns>
    [HttpGet("departments")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<string>>> GetDepartments(CancellationToken cancellationToken = default)
    {
        var departments = await _jobPositionService.GetDepartmentsAsync(cancellationToken);
        return Ok(departments);
    }

    /// <summary>
    /// Creates a new job position.
    /// </summary>
    /// <param name="request">The request containing job position details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created job position.</returns>
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<JobPositionDto>> CreateJobPosition(
        [FromBody] CreateJobPositionRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var result = await _jobPositionService.CreateAsync(request, cancellationToken);
            
            _logger.LogInformation("Job position created with ID {Id} by user", result.Id);
            
            return CreatedAtAction(
                nameof(GetJobPosition), 
                new { id = result.Id }, 
                result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating job position");
            return StatusCode(500, "An error occurred while creating the job position");
        }
    }

    /// <summary>
    /// Updates an existing job position.
    /// </summary>
    /// <param name="id">The ID of the job position to update.</param>
    /// <param name="request">The request containing updated job position details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated job position, or NotFound if the position doesn't exist.</returns>
    [HttpPut("{id:int}")]
    [Authorize]
    public async Task<ActionResult<JobPositionDto>> UpdateJobPosition(
        int id,
        [FromBody] UpdateJobPositionRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var result = await _jobPositionService.UpdateAsync(id, request, cancellationToken);
            
            if (result == null)
            {
                return NotFound($"Job position with ID {id} not found");
            }

            _logger.LogInformation("Job position {Id} updated by user", id);
            
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating job position {Id}", id);
            return StatusCode(500, "An error occurred while updating the job position");
        }
    }

    /// <summary>
    /// Deletes a job position (marks it as inactive).
    /// </summary>
    /// <param name="id">The ID of the job position to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>NoContent if successful, or NotFound if the position doesn't exist.</returns>
    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<ActionResult> DeleteJobPosition(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var success = await _jobPositionService.DeleteAsync(id, cancellationToken);
            
            if (!success)
            {
                return NotFound($"Job position with ID {id} not found");
            }

            _logger.LogInformation("Job position {Id} deleted by user", id);
            
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting job position {Id}", id);
            return StatusCode(500, "An error occurred while deleting the job position");
        }
    }

    /// <summary>
    /// Checks if a job position with the specified ID exists.
    /// </summary>
    /// <param name="id">The ID of the job position to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the job position exists, false otherwise.</returns>
    [HttpGet("{id:int}/exists")]
    [Authorize]
    public async Task<ActionResult<bool>> CheckJobPositionExists(int id, CancellationToken cancellationToken = default)
    {
        var exists = await _jobPositionService.ExistsAsync(id, cancellationToken);
        return Ok(exists);
    }

    /// <summary>
    /// Validates if a job position with the specified title and department already exists.
    /// </summary>
    /// <param name="title">The title to validate.</param>
    /// <param name="department">The department to validate.</param>
    /// <param name="excludeId">Optional ID to exclude from validation (for updates).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the job position is valid (doesn't exist), false otherwise.</returns>
    [HttpGet("validate")]
    [Authorize]
    public async Task<ActionResult<bool>> ValidateJobPosition(
        [FromQuery] string title,
        [FromQuery] string department,
        [FromQuery] int? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(department))
        {
            return BadRequest("Title and department are required");
        }

        var exists = await _jobPositionService.ExistsByTitleAndDepartmentAsync(title, department, excludeId, cancellationToken);
        return Ok(!exists); // Return true if valid (doesn't exist)
    }
}