using Maliev.CareerService.Application.Services.External;
using Maliev.CareerService.Infrastructure.Data;
using Maliev.CareerService.Domain.Entities;
using TrainingEnrollmentStatus = Maliev.CareerService.Domain.Entities.TrainingEnrollmentStatusConstants;
using EnrollmentType = Maliev.CareerService.Domain.Entities.EnrollmentTypeConstants;
using Microsoft.EntityFrameworkCore;

namespace Maliev.CareerService.Api.BackgroundServices;

/// <summary>
/// Background service that periodically checks for overdue mandatory training and escalates notifications
/// </summary>
public class OverdueTrainingEscalationBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<OverdueTrainingEscalationBackgroundService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="OverdueTrainingEscalationBackgroundService"/> class.
    /// </summary>
    public OverdueTrainingEscalationBackgroundService(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<OverdueTrainingEscalationBackgroundService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Overdue Training Escalation Background Service is starting.");

        // Initial delay
        await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);

        using var timer = new PeriodicTimer(TimeSpan.FromHours(24));

        while (!stoppingToken.IsCancellationRequested &&
               await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await ProcessOverdueTrainingAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing overdue training escalations.");
            }
        }
    }

    /// <summary>
    /// Public method for testing escalation logic
    /// </summary>
    public async Task ProcessOverdueTrainingAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Checking for overdue mandatory training...");

        using var scope = _serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CareerDbContext>();
        var notificationClient = scope.ServiceProvider.GetRequiredService<INotificationServiceClient>();
        var employeeClient = scope.ServiceProvider.GetRequiredService<IEmployeeServiceClient>();

        var now = DateTime.UtcNow;

        // Find mandatory enrollments that are overdue and not completed
        var overdueEnrollmentsQuery = dbContext.EmployeeTrainingEnrollments
            .Include(e => e.TrainingProgram)
            .Where(e => e.EnrollmentType == EnrollmentType.Mandatory &&
                        e.Status != TrainingEnrollmentStatus.Completed &&
                        e.Status != TrainingEnrollmentStatus.Cancelled &&
                        e.DueDate.HasValue &&
                        e.DueDate.Value < now)
            .AsAsyncEnumerable();

        await foreach (var enrollment in overdueEnrollmentsQuery.WithCancellation(cancellationToken))
        {
            try
            {
                var daysOverdue = (now - enrollment.DueDate!.Value).Days;

                // Fetch employee to get manager ID
                var employee = await employeeClient.GetEmployeeAsync(enrollment.EmployeeId, cancellationToken);

                if (employee != null)
                {
                    // Logic for escalation:
                    // 1-7 days: Remind employee
                    // 8-14 days: Notify manager
                    // > 14 days: Notify HR (simulated via notification service escalation)

                    await notificationClient.SendMandatoryTrainingReminderAsync(
                        enrollment.EmployeeId,
                        enrollment.TrainingProgram.ProgramName,
                        enrollment.DueDate.Value,
                        daysOverdue,
                        cancellationToken);

                    _logger.LogInformation("Sent overdue reminder for Employee {EmployeeId}, Training {ProgramId}, {Days} days overdue",
                        enrollment.EmployeeId, enrollment.TrainingProgramId, daysOverdue);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process overdue escalation for Enrollment {EnrollmentId}", enrollment.Id);
            }
        }
    }
}
