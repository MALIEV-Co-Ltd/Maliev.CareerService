using AutoMapper;
using Maliev.CareerService.Api.Models.DevelopmentGoals;
using Maliev.CareerService.Data;
using Maliev.CareerService.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Maliev.CareerService.Api.Services;

/// <summary>
/// Service implementation for managing Development Goals
/// </summary>
public class DevelopmentGoalService(
    CareerDbContext dbContext,
    IMapper mapper,
    ILogger<DevelopmentGoalService> logger) : IDevelopmentGoalService
{
    private readonly CareerDbContext _dbContext = dbContext;
    private readonly IMapper _mapper = mapper;
    private readonly ILogger<DevelopmentGoalService> _logger = logger;

    public async Task<DevelopmentGoalResponse> CreateGoalAsync(
        Guid idpId,
        CreateDevelopmentGoalRequest request,
        Guid employeeId,
        CancellationToken cancellationToken = default)
    {
        // Validate IDP exists and belongs to employee
        var idp = await _dbContext.IndividualDevelopmentPlans
            .FirstOrDefaultAsync(i => i.Id == idpId, cancellationToken) ?? throw new InvalidOperationException($"IDP {idpId} not found.");
        if (idp.EmployeeId != employeeId)
        {
            throw new UnauthorizedAccessException("You can only add goals to your own IDPs.");
        }

        var goal = _mapper.Map<EmployeeDevelopmentGoal>(request);
        goal.IdpId = idpId;
        goal.CreatedBy = employeeId;
        goal.UpdatedBy = employeeId;

        _dbContext.EmployeeDevelopmentGoals.Add(goal);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Created goal {GoalId} for IDP {IdpId}",
            goal.Id,
            idpId);

        return _mapper.Map<DevelopmentGoalResponse>(goal);
    }

    public async Task<DevelopmentGoalResponse> UpdateGoalAsync(
        Guid goalId,
        UpdateDevelopmentGoalRequest request,
        Guid employeeId,
        CancellationToken cancellationToken = default)
    {
        var goal = await _dbContext.EmployeeDevelopmentGoals
            .Include(g => g.Idp)
            .FirstOrDefaultAsync(g => g.Id == goalId, cancellationToken) ?? throw new InvalidOperationException($"Goal {goalId} not found.");

        // Verify IDP belongs to employee
        if (goal.Idp.EmployeeId != employeeId)
        {
            throw new UnauthorizedAccessException("You can only update goals in your own IDPs.");
        }

        // Only allow goal edits if IDP status is not Approved (or if just updating status separately)
        if (goal.Idp.Status == IDPStatus.Approved)
        {
            throw new InvalidOperationException("Cannot edit goals in an Approved IDP. Use status update endpoint to track progress.");
        }

        // Verify row version
        var currentRowVersion = Convert.ToBase64String(goal.RowVersion);
        if (currentRowVersion != request.RowVersion)
        {
            throw new DbUpdateConcurrencyException(
                "The goal has been modified by another user. Please refresh and try again.");
        }

        // Update fields
        goal.GoalTitle = request.GoalTitle;
        goal.GoalDescription = request.GoalDescription;
        goal.Category = request.Category;
        goal.TargetDate = request.TargetDate;
        goal.ActionItems = request.ActionItems;
        goal.ProgressNotes = request.ProgressNotes;
        goal.UpdatedBy = employeeId;
        goal.UpdatedAt = DateTime.UtcNow;

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new DbUpdateConcurrencyException(
                "The goal has been modified by another user. Please refresh and try again.");
        }

        _logger.LogInformation("Updated goal {GoalId}", goalId);

        return _mapper.Map<DevelopmentGoalResponse>(goal);
    }

    public async Task<DevelopmentGoalResponse> UpdateGoalStatusAsync(
        Guid goalId,
        UpdateGoalStatusRequest request,
        Guid employeeId,
        CancellationToken cancellationToken = default)
    {
        var goal = await _dbContext.EmployeeDevelopmentGoals
            .Include(g => g.Idp)
            .FirstOrDefaultAsync(g => g.Id == goalId, cancellationToken) ?? throw new InvalidOperationException($"Goal {goalId} not found.");

        // Verify IDP belongs to employee
        if (goal.Idp.EmployeeId != employeeId)
        {
            throw new UnauthorizedAccessException("You can only update goals in your own IDPs.");
        }

        // Verify row version
        var currentRowVersion = Convert.ToBase64String(goal.RowVersion);
        if (currentRowVersion != request.RowVersion)
        {
            throw new DbUpdateConcurrencyException(
                "The goal has been modified by another user. Please refresh and try again.");
        }

        // Update status
        goal.Status = request.Status;
        goal.ProgressNotes = request.ProgressNotes;

        // Set completion date if status is Completed
        if (request.Status == DevelopmentGoalStatus.Completed)
        {
            if (!request.CompletionDate.HasValue)
            {
                throw new InvalidOperationException("Completion date is required when marking a goal as completed.");
            }
            goal.CompletionDate = request.CompletionDate;
        }

        goal.UpdatedBy = employeeId;
        goal.UpdatedAt = DateTime.UtcNow;

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new DbUpdateConcurrencyException(
                "The goal has been modified by another user. Please refresh and try again.");
        }

        _logger.LogInformation(
            "Updated goal {GoalId} status to {Status}",
            goalId,
            request.Status);

        return _mapper.Map<DevelopmentGoalResponse>(goal);
    }

    public async Task<List<DevelopmentGoalResponse>> GetGoalsByIDPAsync(
        Guid idpId,
        CancellationToken cancellationToken = default)
    {
        var goals = await _dbContext.EmployeeDevelopmentGoals
            .Where(g => g.IdpId == idpId)
            .OrderBy(g => g.TargetDate)
            .ToListAsync(cancellationToken);

        return _mapper.Map<List<DevelopmentGoalResponse>>(goals);
    }
}
