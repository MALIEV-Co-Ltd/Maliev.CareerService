using Maliev.CareerService.Application.Models;

namespace Maliev.CareerService.Application.Services;
/// <summary>
/// Service interface for WorkLocation operations
/// </summary>

public interface IWorkLocationService
{
    /// <summary>
    /// Retrieves a work location by its identifier.
    /// </summary>
    /// <param name="id">The work location identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The work location if found; otherwise, null.</returns>
    Task<WorkLocationDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    /// <summary>
    /// Retrieves all work locations with optional filtering.
    /// </summary>
    /// <param name="activeOnly">If true, only active locations are returned.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of work locations.</returns>
    Task<IEnumerable<WorkLocationDto>> GetAllAsync(bool activeOnly = true, CancellationToken cancellationToken = default);
    /// <summary>
    /// Creates a new work location.
    /// </summary>
    /// <param name="request">The creation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created work location.</returns>
    Task<WorkLocationDto> CreateAsync(CreateWorkLocationRequest request, CancellationToken cancellationToken = default);
    /// <summary>
    /// Updates an existing work location.
    /// </summary>
    /// <param name="id">The work location identifier.</param>
    /// <param name="request">The update request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated work location if found; otherwise, null.</returns>
    Task<WorkLocationDto?> UpdateAsync(int id, UpdateWorkLocationRequest request, CancellationToken cancellationToken = default);
    /// <summary>
    /// Deletes a work location.
    /// </summary>
    /// <param name="id">The work location identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if deleted; otherwise, false.</returns>
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    /// <summary>
    /// Checks if a work location exists.
    /// </summary>
    /// <param name="id">The work location identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if exists; otherwise, false.</returns>
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
    /// <summary>
    /// Checks if a work location with the same name and city exists.
    /// </summary>
    /// <param name="name">The location name.</param>
    /// <param name="city">The city.</param>
    /// <param name="excludeId">Optional ID to exclude from the check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if exists; otherwise, false.</returns>
    Task<bool> ExistsByNameAndCityAsync(string name, string city, int? excludeId = null, CancellationToken cancellationToken = default);
    /// <summary>
    /// Retrieves all unique cities.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of city names.</returns>
    Task<IEnumerable<string>> GetCitiesAsync(CancellationToken cancellationToken = default);
}
