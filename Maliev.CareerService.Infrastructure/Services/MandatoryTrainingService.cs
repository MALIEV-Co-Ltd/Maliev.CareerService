
using Maliev.CareerService.Application.Models.TrainingRecords;
using Maliev.CareerService.Application.Services.External;
using Maliev.CareerService.Infrastructure.Data;
using Maliev.CareerService.Domain.Entities;
using TrainingEnrollmentStatus = Maliev.CareerService.Domain.Entities.TrainingEnrollmentStatusConstants;
using EnrollmentType = Maliev.CareerService.Domain.Entities.EnrollmentTypeConstants;
using Maliev.CareerService.Application.Services;
using Microsoft.EntityFrameworkCore;

namespace Maliev.CareerService.Infrastructure.Services;

/// <summary>
/// Service implementation for managing mandatory training requirements (Feature 003)
/// </summary>
public class MandatoryTrainingService(
    CareerDbContext dbContext,
    IEmployeeServiceClient employeeServiceClient,
    ILogger<MandatoryTrainingService> logger) : IMandatoryTrainingService
{
    private readonly CareerDbContext _dbContext = dbContext;
    private readonly IEmployeeServiceClient _employeeServiceClient = employeeServiceClient;
    private readonly ILogger<MandatoryTrainingService> _logger = logger;

    /// <inheritdoc />
    public async Task<MandatoryTrainingRequirementDto> CreateRequirementAsync(
        CreateMandatoryRequirementRequest request,
        Guid currentUserId,
        CancellationToken cancellationToken = default)
    {
        // Check if training program exists
        var programExists = await _dbContext.TrainingPrograms.AnyAsync(p => p.Id == request.TrainingProgramId, cancellationToken);
        if (!programExists)
        {
            throw new InvalidOperationException($"Training program {request.TrainingProgramId} not found");
        }

        // Check for duplicate (same program, department, and position)
        var duplicate = await _dbContext.MandatoryTrainingRequirements
            .FirstOrDefaultAsync(r => r.TrainingProgramId == request.TrainingProgramId &&
                                     r.DepartmentId == request.DepartmentId &&
                                     r.PositionId == request.PositionId &&
                                     !r.IsDeleted, cancellationToken);

        if (duplicate != null)
        {
            throw new InvalidOperationException("A mandatory requirement already exists for this program and target group.");
        }

        var requirement = new MandatoryTrainingRequirement
        {
            TrainingProgramId = request.TrainingProgramId,
            DepartmentId = request.DepartmentId,
            PositionId = request.PositionId,
            CompletionDeadlineDays = request.CompletionDeadlineDays,
            RecertificationMonths = request.RecertificationMonths,
            IsActive = true,
            CreatedBy = currentUserId,
            UpdatedBy = currentUserId
        };

        _dbContext.MandatoryTrainingRequirements.Add(requirement);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Mandatory training requirement created: Program {ProgramId}, Target Dept {DeptId}, RequirementId {RecordId}",
            request.TrainingProgramId,
            request.DepartmentId,
            requirement.Id);

        return MapToDto(requirement);
    }

    /// <inheritdoc />
    public async Task<List<MandatoryTrainingRequirementDto>> GetAllRequirementsAsync(
        bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.MandatoryTrainingRequirements
            .Where(r => !r.IsDeleted);

        if (activeOnly)
        {
            query = query.Where(r => r.IsActive);
        }

        var requirements = await query.ToListAsync(cancellationToken);
        return requirements.Select(MapToDto).ToList();
    }

    /// <inheritdoc />
    public async Task<MandatoryTrainingRequirementDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var requirement = await _dbContext.MandatoryTrainingRequirements
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted, cancellationToken);

        return requirement == null ? null : MapToDto(requirement);
    }

    /// <inheritdoc />
    public async Task<MandatoryTrainingRequirementDto?> UpdateRequirementAsync(
        Guid id,
        UpdateMandatoryRequirementRequest request,
        Guid currentUserId,
        CancellationToken cancellationToken = default)
    {
        var requirement = await _dbContext.MandatoryTrainingRequirements
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted, cancellationToken);

        if (requirement == null)
        {
            return null;
        }

        requirement.CompletionDeadlineDays = request.CompletionDeadlineDays;
        requirement.RecertificationMonths = request.RecertificationMonths;
        requirement.IsActive = request.IsActive;
        requirement.UpdatedBy = currentUserId;

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Mandatory training requirement updated: RequirementId {RecordId}",
            id);

        return MapToDto(requirement);
    }

    /// <inheritdoc />
    public async Task<bool> DeactivateRequirementAsync(
        Guid id,
        Guid currentUserId,
        CancellationToken cancellationToken = default)
    {
        var requirement = await _dbContext.MandatoryTrainingRequirements
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted, cancellationToken);

        if (requirement == null)
        {
            return false;
        }

        requirement.IsActive = false;
        requirement.UpdatedBy = currentUserId;

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Mandatory training requirement deactivated: RequirementId {RecordId}",
            id);

        return true;
    }

    /// <inheritdoc />
    public async Task AssignMandatoryTrainingAsync(
        Guid employeeId,
        Guid? departmentId = null,
        Guid? positionId = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Assigning mandatory training for Employee {EmployeeId}", employeeId);

        try
        {
            Guid? effectiveDepartmentId = departmentId;
            Guid? effectivePositionId = positionId;

            // If IDs are missing, fetch them from Employee Service
            if (effectiveDepartmentId == null || effectivePositionId == null)
            {
                var employee = await _employeeServiceClient.GetEmployeeAsync(employeeId, cancellationToken);
                if (employee == null)
                {
                    _logger.LogWarning("Cannot assign mandatory training - employee {EmployeeId} not found in Employee Service", employeeId);
                    return;
                }
                effectiveDepartmentId ??= employee.DepartmentId;
                effectivePositionId ??= employee.PositionId;
            }

            // Find matching requirements
            var requirements = await _dbContext.MandatoryTrainingRequirements
                .Where(r => r.IsActive && !r.IsDeleted)
                .ToListAsync(cancellationToken);

            var applicableRequirements = requirements.Where(r =>
                (r.DepartmentId == null || r.DepartmentId == effectiveDepartmentId) &&
                (r.PositionId == null || r.PositionId == effectivePositionId)).ToList();

            foreach (var req in applicableRequirements)
            {
                // Check if already enrolled
                var exists = await _dbContext.EmployeeTrainingEnrollments
                    .AnyAsync(e => e.EmployeeId == employeeId && e.TrainingProgramId == req.TrainingProgramId, cancellationToken);

                if (!exists)
                {
                    // Calculate due date (assuming current date as base if we don't have hire date in EmployeeResponse)
                    // In a real scenario, we might want to fetch hire date from Employee Service.
                    var dueDate = DateTime.UtcNow.AddDays(req.CompletionDeadlineDays);

                    var enrollment = new EmployeeTrainingEnrollment
                    {
                        EmployeeId = employeeId,
                        TrainingProgramId = req.TrainingProgramId,
                        EnrolledAt = DateTime.UtcNow,
                        DueDate = dueDate,
                        EnrollmentType = EnrollmentType.Mandatory,
                        Status = TrainingEnrollmentStatus.Enrolled
                    };

                    _dbContext.EmployeeTrainingEnrollments.Add(enrollment);
                    _logger.LogInformation("Assigned mandatory training {ProgramId} to Employee {EmployeeId} with due date {DueDate}",
                        req.TrainingProgramId, employeeId, dueDate);
                }
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while assigning mandatory training for Employee {EmployeeId}", employeeId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task DeactivateAssignmentsAsync(
        Guid employeeId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deactivating mandatory training assignments for Employee {EmployeeId}", employeeId);

        var enrollments = await _dbContext.EmployeeTrainingEnrollments
            .Where(e => e.EmployeeId == employeeId && e.EnrollmentType == EnrollmentType.Mandatory && e.Status == TrainingEnrollmentStatus.Enrolled)
            .ToListAsync(cancellationToken);

        foreach (var enrollment in enrollments)
        {
            enrollment.Status = TrainingEnrollmentStatus.Withdrawn; // Or a dedicated status like Deactivated
            _logger.LogInformation("Deactivated mandatory training enrollment {EnrollmentId} for Employee {EmployeeId}", enrollment.Id, employeeId);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static MandatoryTrainingRequirementDto MapToDto(MandatoryTrainingRequirement requirement)
    {
        return new MandatoryTrainingRequirementDto
        {
            Id = requirement.Id,
            TrainingProgramId = requirement.TrainingProgramId,
            DepartmentId = requirement.DepartmentId,
            PositionId = requirement.PositionId,
            CompletionDeadlineDays = requirement.CompletionDeadlineDays,
            RecertificationMonths = requirement.RecertificationMonths,
            IsActive = requirement.IsActive
        };
    }
}
