using Asp.Versioning;
using Maliev.CareerService.Api.Models;
using Maliev.CareerService.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Maliev.CareerService.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("careers/v{version:apiVersion}/locations")]
[EnableRateLimiting("CareerPolicy")]
public class WorkLocationController : ControllerBase
{
    private readonly IWorkLocationService _workLocationService;
    private readonly ILogger<WorkLocationController> _logger;

    public WorkLocationController(
        IWorkLocationService workLocationService,
        ILogger<WorkLocationController> logger)
    {
        _workLocationService = workLocationService;
        _logger = logger;
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<WorkLocationDto>> GetWorkLocation(int id, CancellationToken cancellationToken = default)
    {
        var location = await _workLocationService.GetByIdAsync(id, cancellationToken);
        
        if (location == null)
        {
            return NotFound($"Work location with ID {id} not found");
        }

        return Ok(location);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<WorkLocationDto>>> GetAllWorkLocations(
        [FromQuery] bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        var locations = await _workLocationService.GetAllAsync(activeOnly, cancellationToken);
        return Ok(locations);
    }

    [HttpGet("cities")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<string>>> GetCities(CancellationToken cancellationToken = default)
    {
        var cities = await _workLocationService.GetCitiesAsync(cancellationToken);
        return Ok(cities);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<WorkLocationDto>> CreateWorkLocation(
        [FromBody] CreateWorkLocationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var result = await _workLocationService.CreateAsync(request, cancellationToken);
            
            _logger.LogInformation("Work location created with ID {Id} by user", result.Id);
            
            return CreatedAtAction(
                nameof(GetWorkLocation), 
                new { id = result.Id }, 
                result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating work location");
            return StatusCode(500, "An error occurred while creating the work location");
        }
    }

    [HttpPut("{id:int}")]
    [Authorize]
    public async Task<ActionResult<WorkLocationDto>> UpdateWorkLocation(
        int id,
        [FromBody] UpdateWorkLocationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var result = await _workLocationService.UpdateAsync(id, request, cancellationToken);
            
            if (result == null)
            {
                return NotFound($"Work location with ID {id} not found");
            }

            _logger.LogInformation("Work location {Id} updated by user", id);
            
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating work location {Id}", id);
            return StatusCode(500, "An error occurred while updating the work location");
        }
    }

    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<ActionResult> DeleteWorkLocation(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var success = await _workLocationService.DeleteAsync(id, cancellationToken);
            
            if (!success)
            {
                return NotFound($"Work location with ID {id} not found");
            }

            _logger.LogInformation("Work location {Id} deleted by user", id);
            
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting work location {Id}", id);
            return StatusCode(500, "An error occurred while deleting the work location");
        }
    }

    [HttpGet("{id:int}/exists")]
    [Authorize]
    public async Task<ActionResult<bool>> CheckWorkLocationExists(int id, CancellationToken cancellationToken = default)
    {
        var exists = await _workLocationService.ExistsAsync(id, cancellationToken);
        return Ok(exists);
    }

    [HttpGet("validate")]
    [Authorize]
    public async Task<ActionResult<bool>> ValidateWorkLocation(
        [FromQuery] string name,
        [FromQuery] string city,
        [FromQuery] int? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(city))
        {
            return BadRequest("Name and city are required");
        }

        var exists = await _workLocationService.ExistsByNameAndCityAsync(name, city, excludeId, cancellationToken);
        return Ok(!exists); // Return true if valid (doesn't exist)
    }
}