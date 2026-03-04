using CareerDbContext = Maliev.CareerService.Infrastructure.Data.CareerDbContext;
using Maliev.MessagingContracts.Contracts.Employee;
using Maliev.MessagingContracts;
using Maliev.CareerService.Domain.Entities;
using EnrollmentType = Maliev.CareerService.Domain.Entities.EnrollmentTypeConstants;
using MassTransit;
using MassTransit.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Maliev.CareerService.Tests.Integration.Consumers;

public class EmployeeCreatedEventConsumerTests : IntegrationTestBase
{
    public EmployeeCreatedEventConsumerTests(CareerServiceWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task EmployeeCreatedEvent_ShouldBeConsumedAndAssignTraining()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var deptId = Guid.NewGuid();
        var programId = Guid.NewGuid();

        // Seed a training program and mandatory requirement
        await SeedDatabaseAsync(new TrainingProgram
        {
            Id = programId,
            ProgramName = "Mandatory Security Training",
            ProgramCode = $"SEC-{Guid.NewGuid().ToString()[..8]}",
            DurationHours = 2.0m,
            CreatedBy = Guid.NewGuid(),
            UpdatedBy = Guid.NewGuid()
        });

        await SeedDatabaseAsync(new MandatoryTrainingRequirement
        {
            Id = Guid.NewGuid(),
            TrainingProgramId = programId,
            DepartmentId = deptId,
            CompletionDeadlineDays = 15,
            IsActive = true,
            CreatedBy = Guid.NewGuid(),
            UpdatedBy = Guid.NewGuid()
        });

        // Update mock employee service to return this employee
        var mockEmployeeService = (Mocks.MockEmployeeServiceClient)Factory.Services.GetRequiredService<Api.Services.External.IEmployeeServiceClient>();
        mockEmployeeService.AddEmployee(new Api.Services.External.EmployeeResponse(
            employeeId, "New", "Employee", "new@maliev.com", "IT", "Security", null, deptId, null));

        var harness = Factory.Services.GetRequiredService<ITestHarness>();

        // Act
        var payload = new EmployeeCreatedEventPayload(
            EmployeeId: employeeId,
            EmployeeNumber: "EMP001",
            PrincipalId: Guid.NewGuid(),
            Email: "new@maliev.com",
            FullName: "New Employee",
            StartDate: DateTime.UtcNow,
            DepartmentId: deptId,
            PositionId: null,
            ManagerId: null
        );

        var integrationEvent = new EmployeeCreatedEvent(
            MessageId: Guid.NewGuid(),
            MessageName: "EmployeeCreated",
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
        Assert.True(await harness.Consumed.Any<EmployeeCreatedEvent>());

        // Wait a bit for processing
        await Task.Delay(1000);

        // Verify enrollment created in DB
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CareerDbContext>();
        var enrollment = await dbContext.EmployeeTrainingEnrollments
            .FirstOrDefaultAsync(e => e.EmployeeId == employeeId && e.TrainingProgramId == programId);

        Assert.NotNull(enrollment);
        Assert.Equal(EnrollmentType.Mandatory, enrollment!.EnrollmentType);
    }
}
