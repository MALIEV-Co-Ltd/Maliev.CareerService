using CareerDbContext = Maliev.CareerService.Infrastructure.Data.CareerDbContext;
using Maliev.MessagingContracts.Contracts.Employee;
using Maliev.MessagingContracts;
using Maliev.CareerService.Domain.Entities;
using EnrollmentType = Maliev.CareerService.Domain.Entities.EnrollmentTypeConstants;
using TrainingEnrollmentStatus = Maliev.CareerService.Domain.Entities.TrainingEnrollmentStatusConstants;
using MassTransit;
using MassTransit.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Maliev.CareerService.Tests.Integration.Consumers;

public class EmployeeTerminatedEventConsumerTests : IntegrationTestBase
{
    public EmployeeTerminatedEventConsumerTests(CareerServiceWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task EmployeeTerminatedEvent_ShouldBeConsumedAndDeactivateTraining()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var programId = Guid.NewGuid();

        // Seed a training program and an active enrollment
        await SeedDatabaseAsync(new TrainingProgram
        {
            Id = programId,
            ProgramName = "Security Training",
            ProgramCode = $"SEC-{Guid.NewGuid().ToString()[..8]}",
            DurationHours = 1.0m,
            CreatedBy = Guid.NewGuid(),
            UpdatedBy = Guid.NewGuid()
        });

        var enrollment = new EmployeeTrainingEnrollment
        {
            Id = Guid.NewGuid(),
            EmployeeId = employeeId,
            TrainingProgramId = programId,
            EnrolledAt = DateTime.UtcNow,
            EnrollmentType = EnrollmentType.Mandatory,
            Status = TrainingEnrollmentStatus.Enrolled,
            CreatedBy = Guid.NewGuid(),
            UpdatedBy = Guid.NewGuid()
        };
        await SeedDatabaseAsync(enrollment);

        var harness = Factory.Services.GetRequiredService<ITestHarness>();

        // Act
        var payload = new EmployeeTerminatedEventPayload(
            EmployeeId: employeeId,
            TerminationDate: DateTimeOffset.UtcNow,
            TerminationReason: "Resigned",
            EligibleForRehire: true
        );

        var integrationEvent = new EmployeeTerminatedEvent(
            MessageId: Guid.NewGuid(),
            MessageName: "EmployeeTerminated",
            MessageType: MessageType.Event,
            MessageVersion: "1.0",
            PublishedBy: "EmployeeService",
            ConsumedBy: Array.Empty<string>(),
            CorrelationId: Guid.NewGuid(),
            CausationId: null,
            OccurredAtUtc: DateTimeOffset.UtcNow,
            IsPublic: false,
            Payload: payload
        );

        await harness.Bus.Publish(integrationEvent);

        // Assert
        Assert.True(await harness.Consumed.Any<EmployeeTerminatedEvent>());

        // Wait a bit for processing
        await Task.Delay(1000);

        // Verify enrollment deactivated in DB
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CareerDbContext>();
        var updatedEnrollment = await dbContext.EmployeeTrainingEnrollments
            .FirstOrDefaultAsync(e => e.Id == enrollment.Id);

        Assert.NotNull(updatedEnrollment);
        Assert.Equal(TrainingEnrollmentStatus.Withdrawn, updatedEnrollment!.Status);
    }
}
