using Maliev.CareerService.Api.Models.Applications;
using Maliev.CareerService.Api.Models.Common;

namespace Maliev.CareerService.Api.Services;

/// <summary>
/// Service interface for managing job applications
/// </summary>
public interface IApplicationService
{
    /// <summary>
    /// Submits a new job application
    /// </summary>
    /// <param name="request">Job application submission request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created job application details</returns>
    /// <exception cref="InvalidOperationException">If application deadline has passed or duplicate application exists</exception>
    /// <exception cref="ArgumentException">If file validation fails</exception>
    Task<JobApplicationResponse> SubmitApplicationAsync(
        SubmitJobApplicationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific job application by ID
    /// </summary>
    /// <param name="id">Job application ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Job application details or null if not found</returns>
    Task<JobApplicationResponse?> GetApplicationByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all applications submitted by a specific applicant (by email)
    /// </summary>
    /// <param name="applicantEmail">Applicant's email address</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of applicant's applications</returns>
    Task<JobApplicationListResponse> GetApplicantApplicationsAsync(
        string applicantEmail,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates if a duplicate application exists for the same email and job posting
    /// </summary>
    /// <param name="jobPostingId">Job posting ID</param>
    /// <param name="applicantEmail">Applicant's email address</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if duplicate exists, false otherwise</returns>
    Task<bool> ValidateDuplicateAsync(
        Guid jobPostingId,
        string applicantEmail,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates if the job posting application deadline has not passed
    /// </summary>
    /// <param name="jobPostingId">Job posting ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deadline is valid (not passed), false otherwise</returns>
    /// <exception cref="InvalidOperationException">If job posting not found</exception>
    Task<bool> ValidateDeadlineAsync(
        Guid jobPostingId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the status of a job application
    /// </summary>
    /// <param name="applicationId">Application ID</param>
    /// <param name="request">Status update request</param>
    /// <param name="hrUserId">HR user ID making the change</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated job application details</returns>
    /// <exception cref="InvalidOperationException">If status transition is invalid</exception>
    /// <exception cref="Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException">If optimistic concurrency conflict occurs</exception>
    Task<JobApplicationResponse> UpdateApplicationStatusAsync(
        Guid applicationId,
        UpdateApplicationStatusRequest request,
        Guid hrUserId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the complete status change history for an application
    /// </summary>
    /// <param name="applicationId">Application ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Complete status history ordered by ChangedAt DESC (newest first)</returns>
    Task<StatusHistoryResponse> GetStatusHistoryAsync(
        Guid applicationId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates if a status transition is allowed
    /// </summary>
    /// <param name="fromStatus">Current status</param>
    /// <param name="toStatus">Target status</param>
    /// <returns>True if transition is valid, false otherwise</returns>
    bool ValidateStatusTransition(string fromStatus, string toStatus);
}
