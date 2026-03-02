using Maliev.CareerService.Api.Mapping;
using Maliev.CareerService.Api.Models.DevelopmentPlans;
using Maliev.CareerService.Api.Services.External;
using Maliev.CareerService.Data;
using Maliev.CareerService.Domain.Entities;
using IDPStatus = Maliev.CareerService.Domain.Entities.IDPStatusConstants;
using Microsoft.EntityFrameworkCore;

namespace Maliev.CareerService.Api.Services;

/// <summary>
/// Service implementation for managing Individual Development Plans
/// </summary>
public class DevelopmentPlanService(
    CareerDbContext dbContext,
    IEmployeeServiceClient employeeService,
    ILogger<DevelopmentPlanService> logger) : IDevelopmentPlanService
{
    private readonly CareerDbContext _dbContext = dbContext;
    private readonly IEmployeeServiceClient _employeeService = employeeService;
    private readonly ILogger<DevelopmentPlanService> _logger = logger;
    /// <inheritdoc/>
    public async Task<IDPListResponse> GetEmployeeIDPsAsync(
        Guid employeeId,
        int? planYear = null,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.IndividualDevelopmentPlans
            .Include(idp => idp.Goals)
            .Where(idp => idp.EmployeeId == employeeId);

        if (planYear.HasValue)
        {
            query = query.Where(idp => idp.PlanYear == planYear.Value);
        }

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(idp => idp.Status == status);
        }

        var idps = await query
            .OrderByDescending(idp => idp.PlanYear)
            .ToListAsync(cancellationToken);

        var responses = idps.Select(i => i.ToIDPResponse()).ToList();

        return new IDPListResponse
        {
            Items = responses,
            TotalCount = responses.Count
        };
    }

    /// <inheritdoc/>
    public async Task<IDPResponse?> GetIDPByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var idp = await _dbContext.IndividualDevelopmentPlans
            .Include(i => i.Goals)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

        if (idp == null)
        {
            return null;
        }

        return idp.ToIDPResponse();
    }
    /// <inheritdoc/>
    public async Task<IDPResponse> CreateIDPAsync(
        Guid employeeId,
        CreateIDPRequest request,
        CancellationToken cancellationToken = default)
    {
        // Validate employee exists
        var employee = await _employeeService.GetEmployeeAsync(employeeId, cancellationToken) ?? throw new InvalidOperationException($"Employee {employeeId} not found.");

        // Check for duplicate year
        if (await CheckDuplicateYearAsync(employeeId, request.PlanYear, cancellationToken))
        {
            throw new InvalidOperationException($"An IDP for year {request.PlanYear} already exists for this employee.");
        }

        var idp = request.ToIndividualDevelopmentPlan();
        idp.EmployeeId = employeeId;
        idp.CreatedBy = employeeId;
        idp.UpdatedBy = employeeId;

        _dbContext.IndividualDevelopmentPlans.Add(idp);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Created IDP {IdpId} for employee {EmployeeId} for year {PlanYear}",
            idp.Id,
            employeeId,
            request.PlanYear);

        return idp.ToIDPResponse();
    }
    /// <inheritdoc/>
    public async Task<IDPResponse> UpdateIDPAsync(
        Guid idpId,
        UpdateIDPRequest request,
        Guid employeeId,
        CancellationToken cancellationToken = default)
    {
        var idp = await _dbContext.IndividualDevelopmentPlans
            .Include(i => i.Goals)
            .FirstOrDefaultAsync(i => i.Id == idpId, cancellationToken) ?? throw new InvalidOperationException($"IDP {idpId} not found.");

        // Verify ownership
        if (idp.EmployeeId != employeeId)
        {
            throw new UnauthorizedAccessException("You can only update your own IDPs.");
        }

        // Only allow updates if status is Draft
        if (idp.Status != IDPStatus.Draft)
        {
            throw new InvalidOperationException($"Cannot update IDP with status {idp.Status}. Only Draft IDPs can be updated.");
        }

        // Verify row version
        var currentRowVersion = Convert.ToBase64String(idp.RowVersion);
        if (currentRowVersion != request.RowVersion)
        {
            throw new DbUpdateConcurrencyException(
                "The IDP has been modified by another user. Please refresh and try again.");
        }

        idp.UpdatedBy = employeeId;
        idp.UpdatedAt = DateTime.UtcNow;

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new DbUpdateConcurrencyException(
                "The IDP has been modified by another user. Please refresh and try again.");
        }

        _logger.LogInformation("Updated IDP {IdpId}", idpId);

        return idp.ToIDPResponse();
    }
    /// <inheritdoc/>
    public async Task<IDPResponse> SubmitIDPAsync(
        Guid idpId,
        Guid employeeId,
        CancellationToken cancellationToken = default)
    {
        var idp = await _dbContext.IndividualDevelopmentPlans
            .Include(i => i.Goals)
            .FirstOrDefaultAsync(i => i.Id == idpId, cancellationToken) ?? throw new InvalidOperationException($"IDP {idpId} not found.");

        // Verify ownership
        if (idp.EmployeeId != employeeId)
        {
            throw new UnauthorizedAccessException("You can only submit your own IDPs.");
        }

        // Only allow submission if status is Draft
        if (idp.Status != IDPStatus.Draft)
        {
            throw new InvalidOperationException($"Cannot submit IDP with status {idp.Status}. Only Draft IDPs can be submitted.");
        }

        idp.Status = IDPStatus.Submitted;
        idp.SubmittedAt = DateTime.UtcNow;
        idp.UpdatedBy = employeeId;
        idp.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Submitted IDP {IdpId} for approval", idpId);

        return idp.ToIDPResponse();
    }
    /// <inheritdoc/>
    public async Task<IDPResponse> ApproveIDPAsync(
        Guid idpId,
        ApproveIDPRequest request,
        Guid hrUserId,
        CancellationToken cancellationToken = default)
    {
        var idp = await _dbContext.IndividualDevelopmentPlans
            .Include(i => i.Goals)
            .FirstOrDefaultAsync(i => i.Id == idpId, cancellationToken) ?? throw new InvalidOperationException($"IDP {idpId} not found.");

        // Only allow approval if status is Submitted
        if (idp.Status != IDPStatus.Submitted)
        {
            throw new InvalidOperationException($"Cannot approve IDP with status {idp.Status}. Only Submitted IDPs can be approved.");
        }

        // Verify row version
        var currentRowVersion = Convert.ToBase64String(idp.RowVersion);
        if (currentRowVersion != request.RowVersion)
        {
            throw new DbUpdateConcurrencyException(
                "The IDP has been modified by another user. Please refresh and try again.");
        }

        idp.Status = IDPStatus.Approved;
        idp.ApprovedAt = DateTime.UtcNow;
        idp.ApprovedBy = hrUserId;
        idp.UpdatedBy = hrUserId;
        idp.UpdatedAt = DateTime.UtcNow;

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new DbUpdateConcurrencyException(
                "The IDP has been modified by another user. Please refresh and try again.");
        }

        _logger.LogInformation(
            "Approved IDP {IdpId} by HR user {HrUserId}",
            idpId,
            hrUserId);

        return idp.ToIDPResponse();
    }
    /// <inheritdoc/>
    public async Task<bool> CheckDuplicateYearAsync(
        Guid employeeId,
        int planYear,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.IndividualDevelopmentPlans
            .AnyAsync(i => i.EmployeeId == employeeId && i.PlanYear == planYear, cancellationToken);
    }
}
