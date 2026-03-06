using Maliev.CareerService.Application.Services.External;
using Maliev.CareerService.Infrastructure.Data;
using Maliev.CareerService.Domain.Entities;
using TrainingStatus = Maliev.CareerService.Domain.Entities.TrainingStatus;
using Microsoft.EntityFrameworkCore;

namespace Maliev.CareerService.Api.BackgroundServices;

/// <summary>
/// Background service that periodically checks for expiring certifications and sends reminders
/// </summary>
public class CertificationExpirationReminderBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<CertificationExpirationReminderBackgroundService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CertificationExpirationReminderBackgroundService"/> class.
    /// </summary>
    public CertificationExpirationReminderBackgroundService(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<CertificationExpirationReminderBackgroundService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Certification Expiration Reminder Background Service is starting.");

        // Initial delay to allow other services to start
        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

        using var timer = new PeriodicTimer(TimeSpan.FromHours(24));

        while (!stoppingToken.IsCancellationRequested &&
               await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<CareerDbContext>();
                var notificationClient = scope.ServiceProvider.GetRequiredService<INotificationServiceClient>();

                await ProcessExpirationsInternalAsync(dbContext, notificationClient, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing certification expirations.");
            }
        }
    }

    /// <summary>
    /// Processes expiring certifications and sends reminders (Public for testing)
    /// </summary>
    public async Task ProcessExpirationsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CareerDbContext>();
        var notificationClient = scope.ServiceProvider.GetRequiredService<INotificationServiceClient>();

        await ProcessExpirationsInternalAsync(dbContext, notificationClient, cancellationToken);
    }

    private async Task ProcessExpirationsInternalAsync(
        CareerDbContext dbContext,
        INotificationServiceClient notificationClient,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing certification expirations...");

        try
        {
            var now = DateTime.UtcNow;

            // 1. Update status to Expired for records that reached expiration date
            var expiredRecordsQuery = dbContext.TrainingRecords
                .Where(tr => tr.Status != TrainingStatus.Expired &&
                            tr.ExpirationDate.HasValue &&
                            tr.ExpirationDate.Value <= now)
                .AsAsyncEnumerable();

            var expiredCount = 0;
            await foreach (var record in expiredRecordsQuery.WithCancellation(cancellationToken))
            {
                record.Status = TrainingStatus.Expired;
                expiredCount++;
                _logger.LogInformation("Marked training record {RecordId} as Expired.", record.Id);
            }

            if (expiredCount > 0)
            {
                await dbContext.SaveChangesAsync(cancellationToken);
            }

            // 2. Find records expiring in exactly 30, 60, or 90 days
            // We use a small range around the target date to be safe with execution timing
            var targetDates = new[] { 30, 60, 90 };

            foreach (var days in targetDates)
            {
                var targetDateStart = now.AddDays(days).Date;
                var targetDateEnd = targetDateStart.AddDays(1);

                var expiringRecordsQuery = dbContext.TrainingRecords
                    .Where(tr => tr.Status == TrainingStatus.Completed &&
                                tr.ExpirationDate.HasValue &&
                                tr.ExpirationDate.Value >= targetDateStart &&
                                tr.ExpirationDate.Value < targetDateEnd)
                    .AsAsyncEnumerable();

                await foreach (var record in expiringRecordsQuery.WithCancellation(cancellationToken))
                {
                    try
                    {
                        await notificationClient.SendCertificationReminderAsync(
                            record.EmployeeId,
                            record.CourseName,
                            record.ExpirationDate!.Value,
                            days,
                            cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send expiration reminder for record {RecordId}", record.Id);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing expirations internal logic.");
            throw;
        }
    }
}
