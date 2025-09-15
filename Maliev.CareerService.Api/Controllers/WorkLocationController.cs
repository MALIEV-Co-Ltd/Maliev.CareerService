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

    /// <summary>
    /// Gets a work location by its ID.
    /// </summary>
    /// <param name="id">The ID of the work location to retrieve.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The work location with the specified ID, or NotFound if not found.</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET careers/v1.0/locations/1
    ///
    /// Sample response:
    ///
    ///     {
    ///         "id": 1,
    ///         "name": "Bangkok Office",
    ///         "address": "123 Tech Street",
    ///         "city": "Bangkok",
    ///         "countryId": 1,
    ///         "isRemoteAllowed": true,
    ///         "isHybrid": false,
    ///         "isActive": true,
    ///         "createdDate": "2025-09-15T10:30:00Z",
    ///         "modifiedDate": "2025-09-15T10:30:00Z"
    ///     }
    ///
    /// Error responses:
    ///
    /// 404 Not Found - When the work location with the specified ID does not exist
    /// 500 Internal Server Error - When there is an unexpected error
    /// </remarks>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<WorkLocationDto>> GetWorkLocationById(int id, CancellationToken cancellationToken = default)
    {
        var location = await _workLocationService.GetByIdAsync(id, cancellationToken);
        
        if (location == null)
        {
            return NotFound($"Work location with ID {id} not found");
        }

        return Ok(location);
    }

    /// <summary>
    /// Gets all work locations.
    /// </summary>
    /// <param name="activeOnly">Whether to return only active work locations (default: true).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of work locations.</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET careers/v1.0/locations?activeOnly=true
    ///
    /// Sample response:
    ///
    ///     [
    ///         {
    ///             "id": 1,
    ///             "name": "Bangkok Office",
    ///             "address": "123 Tech Street",
    ///             "city": "Bangkok",
    ///             "countryId": 1,
    ///             "isRemoteAllowed": true,
    ///             "isHybrid": false,
    ///             "isActive": true,
    ///             "createdDate": "2025-09-15T10:30:00Z",
    ///             "modifiedDate": "2025-09-15T10:30:00Z"
    ///         },
    ///         {
    ///             "id": 2,
    ///             "name": "Chiang Mai Office",
    ///             "address": "456 Innovation Avenue",
    ///             "city": "Chiang Mai",
    ///             "countryId": 1,
    ///             "isRemoteAllowed": false,
    ///             "isHybrid": true,
    ///             "isActive": true,
    ///             "createdDate": "2025-09-15T10:30:00Z",
    ///             "modifiedDate": "2025-09-15T10:30:00Z"
    ///         }
    ///     ]
    ///
    /// Query parameters:
    ///
    /// - activeOnly: Optional. When true, returns only active work locations. When false, returns all work locations (both active and inactive).
    ///
    /// Error responses:
    ///
    /// 500 Internal Server Error - When there is an unexpected error
    /// </remarks>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<WorkLocationDto>>> GetAllWorkLocations(
        [FromQuery] bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        var locations = await _workLocationService.GetAllAsync(activeOnly, cancellationToken);
        return Ok(locations);
    }

    /// <summary>
    /// Gets unique cities from all work locations.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of city names.</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET careers/v1.0/locations/cities
    ///
    /// Sample response:
    ///
    ///     [
    ///         "Bangkok",
    ///         "Chiang Mai",
    ///         "Phuket",
    ///         "Hanoi",
    ///         "Ho Chi Minh City"
    ///     ]
    ///
    /// This endpoint returns all unique city names from active work locations, sorted alphabetically.
    ///
    /// Error responses:
    ///
    /// 500 Internal Server Error - When there is an unexpected error
    /// </remarks>
    [HttpGet("cities")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<string>>> GetCities(CancellationToken cancellationToken = default)
    {
        var cities = await _workLocationService.GetCitiesAsync(cancellationToken);
        return Ok(cities);
    }

    /// <summary>
    /// Creates a new work location.
    /// </summary>
    /// <param name="request">The request containing work location details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created work location.</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST careers/v1.0/locations
    ///     {
    ///         "name": "Bangkok Office",
    ///         "address": "123 Tech Street",
    ///         "city": "Bangkok",
    ///         "countryId": 1,
    ///         "isRemoteAllowed": true,
    ///         "isHybrid": false,
    ///         "isActive": true
    ///     }
    ///
    /// Sample response:
    ///
    ///     {
    ///         "id": 1,
    ///         "name": "Bangkok Office",
    ///         "address": "123 Tech Street",
    ///         "city": "Bangkok",
    ///         "countryId": 1,
    ///         "isRemoteAllowed": true,
    ///         "isHybrid": false,
    ///         "isActive": true,
    ///         "createdDate": "2025-09-15T10:30:00Z",
    ///         "modifiedDate": "2025-09-15T10:30:00Z"
    ///     }
    ///
    /// Authentication:
    ///
    /// This endpoint requires authentication with a valid JWT token.
    ///
    /// Request body parameters:
    ///
    /// - name: Required. Work location name (max 100 characters)
    /// - address: Optional. Work location address (max 500 characters)
    /// - city: Required. City name (max 100 characters)
    /// - countryId: Optional. Country ID
    /// - isRemoteAllowed: Optional. Whether remote work is allowed (default: false)
    /// - isHybrid: Optional. Whether hybrid work is allowed (default: false)
    /// - isActive: Optional. Whether the work location is active (default: true)
    ///
    /// Error responses:
    ///
    /// 400 Bad Request - When the request body is invalid or missing required fields
    /// 401 Unauthorized - When the request is not authenticated
    /// 500 Internal Server Error - When there is an unexpected error
    /// </remarks>
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
                nameof(GetWorkLocationById), 
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

    /// <summary>
    /// Updates an existing work location.
    /// </summary>
    /// <param name="id">The ID of the work location to update.</param>
    /// <param name="request">The request containing updated work location details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated work location, or NotFound if the work location doesn't exist.</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     PUT careers/v1.0/locations/1
    ///     {
    ///         "name": "Bangkok Main Office",
    ///         "address": "123 Tech Street, Suite 100",
    ///         "city": "Bangkok",
    ///         "countryId": 1,
    ///         "isRemoteAllowed": true,
    ///         "isHybrid": true,
    ///         "isActive": true
    ///     }
    ///
    /// Sample response:
    ///
    ///     {
    ///         "id": 1,
    ///         "name": "Bangkok Main Office",
    ///         "address": "123 Tech Street, Suite 100",
    ///         "city": "Bangkok",
    ///         "countryId": 1,
    ///         "isRemoteAllowed": true,
    ///         "isHybrid": true,
    ///         "isActive": true,
    ///         "createdDate": "2025-09-15T10:30:00Z",
    ///         "modifiedDate": "2025-09-15T11:30:00Z"
    ///     }
    ///
    /// Authentication:
    ///
    /// This endpoint requires authentication with a valid JWT token.
    ///
    /// Request body parameters:
    ///
    /// - name: Required. Work location name (max 100 characters)
    /// - address: Optional. Work location address (max 500 characters)
    /// - city: Required. City name (max 100 characters)
    /// - countryId: Optional. Country ID
    /// - isRemoteAllowed: Optional. Whether remote work is allowed
    /// - isHybrid: Optional. Whether hybrid work is allowed
    /// - isActive: Optional. Whether the work location is active
    ///
    /// Error responses:
    ///
    /// 400 Bad Request - When the request body is invalid or missing required fields
    /// 401 Unauthorized - When the request is not authenticated
    /// 404 Not Found - When the work location with the specified ID does not exist
    /// 500 Internal Server Error - When there is an unexpected error
    /// </remarks>
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

    /// <summary>
    /// Deletes a work location (marks it as inactive).
    /// </summary>
    /// <param name="id">The ID of the work location to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>NoContent if successful, or NotFound if the work location doesn't exist.</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     DELETE careers/v1.0/locations/1
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
    /// 404 Not Found - When the work location with the specified ID does not exist
    /// 500 Internal Server Error - When there is an unexpected error
    /// </remarks>
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

    /// <summary>
    /// Checks if a work location with the specified ID exists.
    /// </summary>
    /// <param name="id">The ID of the work location to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the work location exists, false otherwise.</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET careers/v1.0/locations/1/exists
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
    public async Task<ActionResult<bool>> CheckWorkLocationExists(int id, CancellationToken cancellationToken = default)
    {
        var exists = await _workLocationService.ExistsAsync(id, cancellationToken);
        return Ok(exists);
    }

    /// <summary>
    /// Validates if a work location with the specified name and city already exists.
    /// </summary>
    /// <param name="name">The name to validate.</param>
    /// <param name="city">The city to validate.</param>
    /// <param name="excludeId">Optional ID to exclude from validation (for updates).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the work location is valid (doesn't exist), false otherwise.</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET careers/v1.0/locations/validate?name=Bangkok%20Office&amp;city=Bangkok
    ///
    /// Sample response:
    ///
    ///     true
    ///
    /// Query parameters:
    ///
    /// - name: Required. Work location name to validate
    /// - city: Required. City name to validate
    /// - excludeId: Optional. ID to exclude from validation (for updates)
    ///
    /// Error responses:
    ///
    /// 400 Bad Request - When name or city is missing or invalid
    /// 500 Internal Server Error - When there is an unexpected error
    /// </remarks>
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