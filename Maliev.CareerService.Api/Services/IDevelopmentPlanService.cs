using Maliev.CareerService.Api.Models.DevelopmentPlans;

namespace Maliev.CareerService.Api.Services;

/// <summary>
/// Service interface for managing Individual Development Plans
/// </summary>
public interface IDevelopmentPlanService
{
    /// <summary>
    /// Gets all IDPs for an employee
    /// </summary>
    /// <param name="employeeId">The employee identifier.</param>
    /// <param name="planYear">Optional plan year filter.</param>
    /// <param name="status">Optional status filter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list response of IDPs.</returns>
    Task<IDPListResponse> GetEmployeeIDPsAsync(
        Guid employeeId,
        int? planYear = null,
        string? status = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific IDP by ID
    /// </summary>
    /// <param name="id">The IDP identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The IDP if found; otherwise, null.</returns>
    Task<IDPResponse?> GetIDPByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new IDP for an employee
    /// </summary>
    /// <param name="employeeId">The employee identifier.</param>
    /// <param name="request">The creation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created IDP.</returns>
    Task<IDPResponse> CreateIDPAsync(
        Guid employeeId,
        CreateIDPRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing IDP (only if Status = Draft)
    /// </summary>
    /// <param name="idpId">The IDP identifier.</param>
    /// <param name="request">The update request.</param>
    /// <param name="employeeId">The employee identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated IDP.</returns>
    Task<IDPResponse> UpdateIDPAsync(
        Guid idpId,
        UpdateIDPRequest request,
        Guid employeeId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Submits an IDP for approval (changes status from Draft to Submitted)
    /// </summary>
    /// <param name="idpId">The IDP identifier.</param>
    /// <param name="employeeId">The employee identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The submitted IDP.</returns>
    Task<IDPResponse> SubmitIDPAsync(
        Guid idpId,
        Guid employeeId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Approves an IDP (HR only, changes status from Submitted to Approved)
    /// </summary>
    /// <param name="idpId">The IDP identifier.</param>
    /// <param name="request">The approval request.</param>
    /// <param name="hrUserId">The HR user identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The approved IDP.</returns>
    Task<IDPResponse> ApproveIDPAsync(
        Guid idpId,
        ApproveIDPRequest request,
        Guid hrUserId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an employee already has an IDP for the given year
    /// </summary>
    /// <param name="employeeId">The employee identifier.</param>
    /// <param name="planYear">The plan year.</param>
    /// <param name="cancellationToken">Cancel lation token.</param>
    /// <returns>True if a duplicate exists; otherwise, false.</returns>
    Task<bool> CheckDuplicateYearAsync(
        Guid employeeId,
        int planYear,
        CancellationToken cancellationToken = default);
}
