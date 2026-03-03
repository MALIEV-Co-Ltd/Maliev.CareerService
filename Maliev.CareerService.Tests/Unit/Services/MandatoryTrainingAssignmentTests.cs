using Maliev.CareerService.Api.Services;
using Maliev.CareerService.Api.Services.External;
using Maliev.CareerService.Infrastructure.Data;
using Maliev.CareerService.Domain.Entities;
using EnrollmentType = Maliev.CareerService.Domain.Entities.EnrollmentTypeConstants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Maliev.CareerService.Tests.Unit.Services;

public class MandatoryTrainingAssignmentTests : BaseUnitTests
{
    private readonly Mock<IEmployeeServiceClient> _mockEmployeeClient;
    private readonly Mock<ILogger<MandatoryTrainingService>> _mockLogger;

    public MandatoryTrainingAssignmentTests()
    {
        _mockEmployeeClient = new Mock<IEmployeeServiceClient>();
        _mockLogger = new Mock<ILogger<MandatoryTrainingService>>();
    }

    [Fact]
    public async Task AssignMandatoryTrainingAsync_ShouldCreateEnrollments()
    {
        // Arrange
        await using var dbContext = CreateDbContext();

        var employeeId = Guid.NewGuid();
        var deptId = Guid.NewGuid();

        _mockEmployeeClient.Setup(x => x.GetEmployeeAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EmployeeResponse(employeeId, "Test", "User", "test@maliev.com", "Engineering", "Developer", null, deptId, null));

        var programId = Guid.NewGuid();
        dbContext.TrainingPrograms.Add(new TrainingProgram { Id = programId, ProgramName = "Compliance", DurationHours = 1 });

        dbContext.MandatoryTrainingRequirements.Add(new MandatoryTrainingRequirement
        {
            Id = Guid.NewGuid(),
            TrainingProgramId = programId,
            DepartmentId = deptId,
            CompletionDeadlineDays = 30,
            IsActive = true
        });

        await dbContext.SaveChangesAsync();

        var service = new MandatoryTrainingService(dbContext, _mockEmployeeClient.Object, _mockLogger.Object);

        // Act
        await service.AssignMandatoryTrainingAsync(employeeId, cancellationToken: CancellationToken.None);

        // Assert
        var enrollments = await dbContext.EmployeeTrainingEnrollments.Where(e => e.EmployeeId == employeeId).ToListAsync();
        Assert.Single(enrollments);
        Assert.Equal(programId, enrollments[0].TrainingProgramId);
        Assert.Equal(EnrollmentType.Mandatory, enrollments[0].EnrollmentType);
        Assert.NotNull(enrollments[0].DueDate);
    }
}
