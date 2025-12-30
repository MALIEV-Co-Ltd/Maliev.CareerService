using FluentAssertions;
using Maliev.CareerService.Data.Events;
using Maliev.CareerService.Data.Models;
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
            ProgramCode = "SEC-001",
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
        await harness.Bus.Publish(new EmployeeCreatedEvent(employeeId, "new@maliev.com", deptId, null, DateTime.UtcNow));

        // Assert
        (await harness.Consumed.Any<EmployeeCreatedEvent>()).Should().BeTrue();

        // Wait a bit for processing
        await Task.Delay(1000);

        // Verify enrollment created in DB
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<Data.CareerDbContext>();
        var enrollment = await dbContext.EmployeeTrainingEnrollments
            .FirstOrDefaultAsync(e => e.EmployeeId == employeeId && e.TrainingProgramId == programId);

        enrollment.Should().NotBeNull();
        enrollment!.EnrollmentType.Should().Be(EnrollmentType.Mandatory);
    }
}
