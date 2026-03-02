
using Maliev.CareerService.Api.Mapping;
using Maliev.CareerService.Api.Models.TrainingRecords;
using Maliev.CareerService.Data;
using Maliev.CareerService.Domain.Entities;
using Maliev.MessagingContracts.Contracts.Career;
using Maliev.MessagingContracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;


namespace Maliev.CareerService.Api.Services;

/// <summary>
/// Service implementation for managing training records (Feature 003)
/// </summary>
public class TrainingRecordService(
    CareerDbContext dbContext,
    IPublishEndpoint publishEndpoint,
    ILogger<TrainingRecordService> logger) : ITrainingRecordService
{
    private readonly CareerDbContext _dbContext = dbContext;
    private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;
    private readonly ILogger<TrainingRecordService> _logger = logger;

    /// <inheritdoc />
    public async Task<TrainingRecordResponse> RecordCompletionAsync(
        Guid employeeId,
        RecordTrainingCompletionRequest request,
        Guid currentUserId,
        CancellationToken cancellationToken = default)
    {
        // Business rule validation: Completion date cannot be in the future
        if (request.CompletionDate > DateTime.UtcNow)
        {
            throw new InvalidOperationException("Completion date cannot be in the future");
        }

        // Business rule validation: Expiration date must be after completion date
        if (request.ExpirationDate.HasValue && request.ExpirationDate.Value <= request.CompletionDate)
        {
            throw new InvalidOperationException("Expiration date must be after completion date");
        }

        var record = request.ToTrainingRecord(employeeId);

        // Set audit fields (per research.md - manual population at service layer)
        record.CreatedBy = currentUserId;
        record.UpdatedBy = currentUserId;

        _dbContext.TrainingRecords.Add(record);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Training completion recorded: Employee {EmployeeId}, Course {CourseName}, RecordId {RecordId}",
            employeeId,
            request.CourseName,
            record.Id);

        // Publish integration event
        await _publishEndpoint.Publish(new TrainingCompletedEvent(
            MessageId: Guid.NewGuid(),
            MessageName: nameof(TrainingCompletedEvent),
            MessageType: MessageType.Event,
            MessageVersion: "1.0",
            PublishedBy: "Maliev.CareerService",
            ConsumedBy: new[] { "Maliev.ComplianceService" },
            CorrelationId: Guid.NewGuid(),
            CausationId: null,
            OccurredAtUtc: DateTimeOffset.UtcNow,
            IsPublic: true,
            Payload: new TrainingCompletedEventPayload(
                TrainingRecordId: record.Id,
                EmployeeId: record.EmployeeId,
                CourseName: record.CourseName,
                CompletionDate: record.CompletionDate,
                CertificationExpiration: record.ExpirationDate
            )
        ), cancellationToken);

        // If it has an expiration date, it's a certification
        if (record.ExpirationDate.HasValue)
        {
            await _publishEndpoint.Publish(new CertificationAwardedEvent(
                MessageId: Guid.NewGuid(),
                MessageName: nameof(CertificationAwardedEvent),
                MessageType: MessageType.Event,
                MessageVersion: "1.0",
                PublishedBy: "Maliev.CareerService",
                ConsumedBy: Array.Empty<string>(),
                CorrelationId: Guid.NewGuid(),
                CausationId: null,
                OccurredAtUtc: DateTimeOffset.UtcNow,
                IsPublic: false,
                Payload: new CertificationAwardedEventPayload(
                    CertificationId: record.Id,
                    EmployeeId: record.EmployeeId,
                    CertificationName: record.CourseName,
                    AwardedDate: record.CompletionDate,
                    ExpirationDate: record.ExpirationDate
                )
            ), cancellationToken);
        }

        return record.ToTrainingRecordResponse();
    }

    /// <inheritdoc />
    public async Task<TrainingRecordListResponse> GetByEmployeeIdAsync(
        Guid employeeId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.TrainingRecords
            .Where(tr => tr.EmployeeId == employeeId)
            .OrderByDescending(tr => tr.CompletionDate);

        var totalCount = await query.CountAsync(cancellationToken);

        var records = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var responses = records.Select(r => r.ToTrainingRecordResponse()).ToList();

        return new TrainingRecordListResponse
        {
            Items = responses,
            Page = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    /// <inheritdoc />
    public async Task<TrainingRecordResponse?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var record = await _dbContext.TrainingRecords
            .FirstOrDefaultAsync(tr => tr.Id == id, cancellationToken);

        return record?.ToTrainingRecordResponse();
    }

    /// <inheritdoc />
    public async Task<TrainingRecordResponse?> UpdateAsync(
        Guid id,
        UpdateTrainingRecordRequest request,
        Guid currentUserId,
        CancellationToken cancellationToken = default)
    {
        var record = await _dbContext.TrainingRecords
            .FirstOrDefaultAsync(tr => tr.Id == id, cancellationToken);

        if (record == null)
        {
            return null;
        }

        // Business rule validation: Expiration date must be after completion date
        if (request.ExpirationDate.HasValue && request.ExpirationDate.Value <= record.CompletionDate)
        {
            throw new InvalidOperationException("Expiration date must be after completion date");
        }

        record.UpdateTrainingRecord(request);
        record.UpdatedBy = currentUserId;

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Training record updated: RecordId {RecordId}, Employee {EmployeeId}",
            id,
            record.EmployeeId);

        return record.ToTrainingRecordResponse();
    }

    /// <inheritdoc />
    public async Task<TrainingRecordListResponse> GetExpiringAsync(
        int days,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var targetDate = DateTime.UtcNow.AddDays(days);

        var query = _dbContext.TrainingRecords
            .Where(tr => tr.Status == TrainingStatus.Completed &&
                        tr.ExpirationDate.HasValue &&
                        tr.ExpirationDate.Value <= targetDate)
            .OrderBy(tr => tr.ExpirationDate);

        var totalCount = await query.CountAsync(cancellationToken);

        var records = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var responses = records.Select(r => r.ToTrainingRecordResponse()).ToList();

        return new TrainingRecordListResponse
        {
            Items = responses,
            Page = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(
        Guid id,
        Guid currentUserId,
        CancellationToken cancellationToken = default)
    {
        var record = await _dbContext.TrainingRecords
            .FirstOrDefaultAsync(tr => tr.Id == id, cancellationToken);

        if (record == null)
        {
            return false;
        }

        // Soft delete
        record.IsDeleted = true;
        record.UpdatedBy = currentUserId;

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Training record deleted (soft): RecordId {RecordId}, Employee {EmployeeId}",
            id,
            record.EmployeeId);

        return true;
    }
}
