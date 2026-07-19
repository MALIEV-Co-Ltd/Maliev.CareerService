using Maliev.CareerService.Application.Models;

namespace Maliev.CareerService.Application.Services;
/// <summary>
/// Service interface for JobPosition operations
/// </summary>

public interface IJobPositionService
{
    /// <summary>
    /// Retrieves a job position by its identifier.
    /// </summary>
    /// <param name="id">The job position identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The job position if found; otherwise, null.</returns>
    Task<JobPositionDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    /// <summary>
    /// Searches for job positions based on search criteria.
    /// </summary>
    /// <param name="request">The search request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paged result of job positions.</returns>
    Task<PagedResult<JobPositionDto>> SearchAsync(JobPositionSearchRequest request, CancellationToken cancellationToken = default);
    /// <summary>
    /// Creates a new job position.
    /// </summary>
    /// <param name="request">The creation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created job position.</returns>
    Task<JobPositionDto> CreateAsync(CreateJobPositionRequest request, CancellationToken cancellationToken = default);
    /// <summary>
    /// Updates an existing job position.
    /// </summary>
    /// <param name="id">The job position identifier.</param>
    /// <param name="request">The update request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated job position if found; otherwise, null.</returns>
    Task<JobPositionDto?> UpdateAsync(int id, UpdateJobPositionRequest request, CancellationToken cancellationToken = default);
    /// <summary>
    /// Deletes a job position.
    /// </summary>
    /// <param name="id">The job position identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if deleted; otherwise, false.</returns>
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    /// <summary>
    /// Checks if a job position exists.
    /// </summary>
    /// <param name="id">The job position identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if exists; otherwise, false.</returns>
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
    /// <summary>
    /// Checks if a job position with the same title and department exists.
    /// </summary>
    /// <param name="title">The job title.</param>
    /// <param name="department">The department.</param>
    /// <param name="excludeId">Optional ID to exclude from the check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if exists; otherwise, false.</returns>
    Task<bool> ExistsByTitleAndDepartmentAsync(string title, string department, int? excludeId = null, CancellationToken cancellationToken = default);
    /// <summary>
    /// Retrieves all unique departments.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of department names.</returns>
    Task<IEnumerable<string>> GetDepartmentsAsync(CancellationToken cancellationToken = default);
    /// <summary>
    /// Retrieves publicly visible job positions based on search criteria.
    /// </summary>
    /// <param name="request">The search request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paged result of public job positions.</returns>
    Task<PagedResult<JobPositionDto>> GetPublicPositionsAsync(JobPositionSearchRequest request, CancellationToken cancellationToken = default);
}
