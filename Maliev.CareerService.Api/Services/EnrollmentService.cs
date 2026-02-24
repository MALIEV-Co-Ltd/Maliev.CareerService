using Maliev.CareerService.Api.Mapping;
using Maliev.CareerService.Api.Models.Enrollments;
using Maliev.CareerService.Api.Services.External;
using Maliev.CareerService.Data;
using Maliev.CareerService.Data.Models;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Maliev.CareerService.Api.Services;

/// <summary>
/// Service implementation for managing training enrollments
/// </summary>
public class EnrollmentService(
    CareerDbContext dbContext,
    IEmployeeServiceClient employeeService,
    IMetricsService metricsService,
    IPublishEndpoint publishEndpoint,
    ILogger<EnrollmentService> logger) : IEnrollmentService
{
    private readonly CareerDbContext _dbContext = dbContext;
    private readonly IEmployeeServiceClient _employeeService = employeeService;
    private readonly IMetricsService _metricsService = metricsService;
    private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;
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

        var enrollment = request.ToEmployeeTrainingEnrollment();
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

        // Publish TrainingEnrolledEvent
        await _publishEndpoint.Publish(new Maliev.MessagingContracts.Generated.TrainingEnrolledEvent(
            MessageId: Guid.NewGuid(),
            MessageName: nameof(Maliev.MessagingContracts.Generated.TrainingEnrolledEvent),
            MessageType: Maliev.MessagingContracts.Generated.MessageType.Event,
            MessageVersion: "1.0",
            PublishedBy: "CareerService",
            ConsumedBy: Array.Empty<string>(),
            CorrelationId: Guid.NewGuid(),
            CausationId: null,
            OccurredAtUtc: DateTimeOffset.UtcNow,
            IsPublic: false,
            Payload: new Maliev.MessagingContracts.Generated.TrainingEnrolledEventPayload(
                TrainingRecordId: enrollment.Id,
                EmployeeId: enrollment.EmployeeId,
                CourseName: trainingProgram.ProgramName,
                EnrollmentDate: enrollment.EnrolledAt
            )
        ), cancellationToken);

        // Track metrics
        _metricsService.IncrementTrainingEnrollments(enrollment.Status);

        _logger.LogInformation("Employee {EmployeeId} enrolled in training program {ProgramId}", employeeId, request.TrainingProgramId);

        return enrollment.ToTrainingEnrollmentResponse();
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
            var response = e.ToTrainingEnrollmentResponse();
            response.TrainingProgram = e.TrainingProgram.ToTrainingProgramResponse();
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

        // Attach the provided RowVersion to the tracked entity for optimistic concurrency
        _dbContext.Entry(enrollment).Property(e => e.RowVersion).OriginalValue = Convert.FromBase64String(request.RowVersion);

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

            // Calculate certification expiration if program has ValidityMonths
            DateTime? certificationExpiration = null;
            if (enrollment.TrainingProgram.ValidityMonths.HasValue && enrollment.CompletedAt.HasValue)
            {
                certificationExpiration = enrollment.CompletedAt.Value.AddMonths(enrollment.TrainingProgram.ValidityMonths.Value);
            }

            // Publish TrainingCompletedEvent
            await _publishEndpoint.Publish(new Maliev.MessagingContracts.Generated.TrainingCompletedEvent(
                MessageId: Guid.NewGuid(),
                MessageName: nameof(Maliev.MessagingContracts.Generated.TrainingCompletedEvent),
                MessageType: Maliev.MessagingContracts.Generated.MessageType.Event,
                MessageVersion: "1.0",
                PublishedBy: "CareerService",
                ConsumedBy: Array.Empty<string>(),
                CorrelationId: Guid.NewGuid(),
                CausationId: null,
                OccurredAtUtc: DateTimeOffset.UtcNow,
                IsPublic: false,
                Payload: new Maliev.MessagingContracts.Generated.TrainingCompletedEventPayload(
                    TrainingRecordId: enrollment.Id,
                    EmployeeId: enrollment.EmployeeId,
                    CourseName: enrollment.TrainingProgram.ProgramName,
                    CompletionDate: enrollment.CompletedAt.Value,
                    CertificationExpiration: certificationExpiration
                )
            ), cancellationToken);

            // Track metrics
            _metricsService.IncrementTrainingEnrollments(enrollment.Status);

            _logger.LogInformation("Enrollment {EnrollmentId} marked as completed by {UserId}", id, markedCompleteBy);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict when marking enrollment {EnrollmentId} as completed", id);
            throw;
        }

        var response = enrollment.ToTrainingEnrollmentResponse();
        response.TrainingProgram = enrollment.TrainingProgram.ToTrainingProgramResponse();
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
