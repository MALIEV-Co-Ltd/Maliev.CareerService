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
    Task<IDPListResponse> GetEmployeeIDPsAsync(
        Guid employeeId,
        int? planYear = null,
        string? status = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific IDP by ID
    /// </summary>
    Task<IDPResponse?> GetIDPByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new IDP for an employee
    /// </summary>
    Task<IDPResponse> CreateIDPAsync(
        Guid employeeId,
        CreateIDPRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing IDP (only if Status = Draft)
    /// </summary>
    Task<IDPResponse> UpdateIDPAsync(
        Guid idpId,
        UpdateIDPRequest request,
        Guid employeeId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Submits an IDP for approval (changes status from Draft to Submitted)
    /// </summary>
    Task<IDPResponse> SubmitIDPAsync(
        Guid idpId,
        Guid employeeId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Approves an IDP (HR only, changes status from Submitted to Approved)
    /// </summary>
    Task<IDPResponse> ApproveIDPAsync(
        Guid idpId,
        ApproveIDPRequest request,
        Guid hrUserId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an employee already has an IDP for the given year
    /// </summary>
    Task<bool> CheckDuplicateYearAsync(
        Guid employeeId,
        int planYear,
        CancellationToken cancellationToken = default);
}
