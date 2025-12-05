using Maliev.CareerService.Api.Models;

namespace Maliev.CareerService.Api.Services;
/// <summary>
/// Service interface for JobApplication operations
/// </summary>

public interface IJobApplicationService
{
    /// <summary>
    /// Retrieves a job application by its identifier.
    /// </summary>
    /// <param name="id">The application identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The application if found; otherwise, null.</returns>
    Task<JobApplicationDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    /// <summary>
    /// Retrieves all applications for a specific job position with pagination.
    /// </summary>
    /// <param name="jobPositionId">The job position identifier.</param>
    /// <param name="page">The page number.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paged result of applications.</returns>
    Task<PagedResult<JobApplicationDto>> GetByJobPositionIdAsync(int jobPositionId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    /// <summary>
    /// Retrieves all applications with optional filtering and pagination.
    /// </summary>
    /// <param name="page">The page number.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="status">Optional status filter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paged result of applications.</returns>
    Task<PagedResult<JobApplicationDto>> GetAllAsync(int page = 1, int pageSize = 20, string? status = null, CancellationToken cancellationToken = default);
    /// <summary>
    /// Creates a new job application.
    /// </summary>
    /// <param name="request">The creation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created application.</returns>
    Task<JobApplicationDto> CreateAsync(CreateJobApplicationRequest request, CancellationToken cancellationToken = default);
    /// <summary>
    /// Updates the status of a job application.
    /// </summary>
    /// <param name="id">The application identifier.</param>
    /// <param name="request">The update request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated application if found; otherwise, null.</returns>
    Task<JobApplicationDto?> UpdateStatusAsync(int id, UpdateJobApplicationRequest request, CancellationToken cancellationToken = default);
    /// <summary>
    /// Deletes a job application.
    /// </summary>
    /// <param name="id">The application identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if deleted; otherwise, false.</returns>
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    /// <summary>
    /// Checks if a job application exists.
    /// </summary>
    /// <param name="id">The application identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if exists; otherwise, false.</returns>
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
    /// <summary>
    /// Checks if an applicant has already applied to a job position.
    /// </summary>
    /// <param name="email">The applicant email.</param>
    /// <param name="jobPositionId">The job position identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if application exists; otherwise, false.</returns>
    Task<bool> HasExistingApplicationAsync(string email, int jobPositionId, CancellationToken cancellationToken = default);
    /// <summary>
    /// Retrieves all applications for a specific email address.
    /// </summary>
    /// <param name="email">The applicant email.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of applications.</returns>
    Task<IEnumerable<JobApplicationDto>> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
}
