using AutoMapper;
using Maliev.CareerService.Api.Models.Enrollments;
using Maliev.CareerService.Api.Services.External;
using Maliev.CareerService.Data;
using Maliev.CareerService.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Maliev.CareerService.Api.Services;

/// <summary>
/// Service implementation for managing training enrollments
/// </summary>
public class EnrollmentService(
    CareerDbContext dbContext,
    IMapper mapper,
    IEmployeeServiceClient employeeService,
    IMetricsService metricsService,
    ILogger<EnrollmentService> logger) : IEnrollmentService
{
    private readonly CareerDbContext _dbContext = dbContext;
    private readonly IMapper _mapper = mapper;
    private readonly IEmployeeServiceClient _employeeService = employeeService;
    private readonly IMetricsService _metricsService = metricsService;
    private readonly ILogger<EnrollmentService> _logger = logger;

    /// <inheritdoc />
    public async Task<TrainingEnrollmentResponse> EnrollEmployeeAsync(
        EnrollInTrainingRequest request,
        Guid employeeId,
        CancellationToken cancellationToken = default)
    {
        // Validate employee exists
        var employeeValid = await _employeeService.ValidateEmployeeAsync(employeeId, cancellationToken);
        if (!employeeValid)
        {
            throw new InvalidOperationException($"Employee {employeeId} not found.");
        }

        // Validate training program exists
        var trainingProgram = await _dbContext.TrainingPrograms
            .FirstOrDefaultAsync(tp => tp.Id == request.TrainingProgramId, cancellationToken) ?? throw new InvalidOperationException($"Training program {request.TrainingProgramId} not found.");
        if (!trainingProgram.IsActive)
        {
            throw new InvalidOperationException($"Training program {request.TrainingProgramId} is not active.");
        }

        // Check for duplicate enrollment
        var isDuplicate = await CheckDuplicateEnrollmentAsync(request.TrainingProgramId, employeeId, cancellationToken);
        if (isDuplicate)
        {
            throw new InvalidOperationException($"Employee {employeeId} is already enrolled in training program {request.TrainingProgramId}.");
        }

        // Validate capacity
        var hasCapacity = await ValidateCapacityAsync(request.TrainingProgramId, cancellationToken);
        if (!hasCapacity)
        {
            throw new InvalidOperationException($"Training program {request.TrainingProgramId} has reached maximum capacity.");
        }

        var enrollment = _mapper.Map<EmployeeTrainingEnrollment>(request);
        enrollment.EmployeeId = employeeId;
        enrollment.EnrolledAt = DateTime.UtcNow;

        // Determine enrollment type based on training program
        enrollment.EnrollmentType = trainingProgram.IsMandatory
            ? Data.Models.EnrollmentType.Mandatory
            : Data.Models.EnrollmentType.Voluntary;

        enrollment.CreatedBy = employeeId;
        enrollment.UpdatedBy = employeeId;

        _dbContext.EmployeeTrainingEnrollments.Add(enrollment);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Track metrics
        _metricsService.IncrementTrainingEnrollments(enrollment.Status);

        _logger.LogInformation("Employee {EmployeeId} enrolled in training program {ProgramId}", employeeId, request.TrainingProgramId);

        return _mapper.Map<TrainingEnrollmentResponse>(enrollment);
    }

    /// <inheritdoc />
    public async Task<TrainingEnrollmentListResponse> GetEmployeeEnrollmentsAsync(
        Guid employeeId,
        string? status,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.EmployeeTrainingEnrollments
            .Include(e => e.TrainingProgram)
            .Where(e => e.EmployeeId == employeeId);

        // Apply status filter
        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(e => e.Status == status);
        }

        query = query.OrderByDescending(e => e.EnrolledAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var enrollments = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var responses = enrollments.Select(e =>
        {
            var response = _mapper.Map<TrainingEnrollmentResponse>(e);
            response.TrainingProgram = _mapper.Map<Models.TrainingPrograms.TrainingProgramResponse>(e.TrainingProgram);
            return response;
        }).ToList();

        return new TrainingEnrollmentListResponse
        {
            Items = responses,
            Page = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    /// <inheritdoc />
    public async Task<TrainingEnrollmentResponse?> MarkCompletedAsync(
        Guid id,
        MarkTrainingCompleteRequest request,
        Guid markedCompleteBy,
        CancellationToken cancellationToken = default)
    {
        var enrollment = await _dbContext.EmployeeTrainingEnrollments
            .Include(e => e.TrainingProgram)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        if (enrollment == null)
        {
            return null;
        }

        // Verify RowVersion for optimistic concurrency
        var requestRowVersion = Convert.FromBase64String(request.RowVersion);
        if (!enrollment.RowVersion.SequenceEqual(requestRowVersion))
        {
            throw new DbUpdateConcurrencyException("The enrollment has been modified by another user. Please refresh and try again.");
        }

        // Update enrollment status
        enrollment.Status = TrainingEnrollmentStatus.Completed;
        enrollment.CompletedAt = DateTime.UtcNow;
        enrollment.CompletionNotes = request.CompletionNotes;
        enrollment.MarkedCompleteBy = markedCompleteBy;
        enrollment.UpdatedBy = markedCompleteBy;

        // Set StartedAt if not already set
        if (!enrollment.StartedAt.HasValue)
        {
            enrollment.StartedAt = enrollment.EnrolledAt;
        }

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);

            // Track metrics
            _metricsService.IncrementTrainingEnrollments(enrollment.Status);

            _logger.LogInformation("Enrollment {EnrollmentId} marked as completed by {UserId}", id, markedCompleteBy);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict when marking enrollment {EnrollmentId} as completed", id);
            throw;
        }

        var response = _mapper.Map<TrainingEnrollmentResponse>(enrollment);
        response.TrainingProgram = _mapper.Map<Models.TrainingPrograms.TrainingProgramResponse>(enrollment.TrainingProgram);
        return response;
    }

    /// <inheritdoc />
    public async Task<bool> ValidateCapacityAsync(
        Guid trainingProgramId,
        CancellationToken cancellationToken = default)
    {
        var trainingProgram = await _dbContext.TrainingPrograms
            .FirstOrDefaultAsync(tp => tp.Id == trainingProgramId, cancellationToken);

        if (trainingProgram == null)
        {
            return false;
        }

        // If no capacity limit, return true
        if (!trainingProgram.MaxParticipants.HasValue)
        {
            return true;
        }

        // Count active enrollments (not cancelled)
        var activeEnrollments = await _dbContext.EmployeeTrainingEnrollments
            .CountAsync(e => e.TrainingProgramId == trainingProgramId && e.Status != TrainingEnrollmentStatus.Cancelled, cancellationToken);

        return activeEnrollments < trainingProgram.MaxParticipants.Value;
    }

    /// <inheritdoc />
    public async Task<bool> CheckDuplicateEnrollmentAsync(
        Guid trainingProgramId,
        Guid employeeId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.EmployeeTrainingEnrollments
            .AnyAsync(e => e.TrainingProgramId == trainingProgramId && e.EmployeeId == employeeId, cancellationToken);
    }
}
