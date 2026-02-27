using Maliev.CareerService.Api.Models.Enrollments;
using Maliev.CareerService.Api.Services;
using Maliev.CareerService.Api.Services.External;
using Maliev.CareerService.Data;
using Maliev.CareerService.Data.Models;
using Maliev.MessagingContracts.Contracts.Career;
using Maliev.MessagingContracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using MassTransit;
using Xunit;

namespace Maliev.CareerService.Tests.Unit.Services;

public class EnrollmentServiceTests : BaseUnitTests
{
    private readonly Mock<IEmployeeServiceClient> _mockEmployeeClient;
    private readonly Mock<IMetricsService> _mockMetricsService;
    private readonly Mock<IPublishEndpoint> _mockPublishEndpoint;
    private readonly Mock<ILogger<EnrollmentService>> _mockLogger;

    public EnrollmentServiceTests()
    {
        _mockEmployeeClient = new Mock<IEmployeeServiceClient>();
        _mockMetricsService = new Mock<IMetricsService>();
        _mockPublishEndpoint = new Mock<IPublishEndpoint>();
        _mockLogger = new Mock<ILogger<EnrollmentService>>();
    }

    [Fact]
    public async Task MarkCompletedAsync_CalculatesCertificationExpiration()
    {
        // Arrange
        await using var dbContext = CreateDbContext();

        var employeeId = Guid.NewGuid();
        var programId = Guid.NewGuid();
        var validityMonths = 12;

        var program = new TrainingProgram
        {
            Id = programId,
            ProgramName = "Certified Kubernetes Administrator",
            ValidityMonths = validityMonths,
            DurationHours = 40
        };
        dbContext.TrainingPrograms.Add(program);

        var enrollment = new EmployeeTrainingEnrollment
        {
            Id = Guid.NewGuid(),
            EmployeeId = employeeId,
            TrainingProgramId = programId,
            Status = TrainingEnrollmentStatus.InProgress,
            EnrolledAt = DateTime.UtcNow.AddDays(-5),
            EnrollmentType = EnrollmentType.Voluntary,
            RowVersion = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }
        };
        dbContext.EmployeeTrainingEnrollments.Add(enrollment);
        await dbContext.SaveChangesAsync();

        var service = new EnrollmentService(
            dbContext,
            _mockEmployeeClient.Object,
            _mockMetricsService.Object,
            _mockPublishEndpoint.Object,
            _mockLogger.Object);

        var request = new MarkTrainingCompleteRequest
        {
            CompletionNotes = "Passed exam with 90%",
            RowVersion = Convert.ToBase64String(enrollment.RowVersion)
        };

        TrainingCompletedEvent? capturedEvent = null;
        _mockPublishEndpoint
            .Setup(x => x.Publish(It.IsAny<TrainingCompletedEvent>(), It.IsAny<CancellationToken>()))
            .Callback<TrainingCompletedEvent, CancellationToken>((e, c) => capturedEvent = e)
            .Returns(Task.CompletedTask);

        // Act
        var result = await service.MarkCompletedAsync(enrollment.Id, request, Guid.NewGuid());

        // Assert
        Assert.NotNull(result);
        Assert.Equal(TrainingEnrollmentStatus.Completed, result.Status);
        Assert.NotNull(result.CompletedAt);

        Assert.NotNull(capturedEvent);
        Assert.NotNull(capturedEvent.Payload.CertificationExpiration);

        // Expiration should be roughly CompletionDate + ValidityMonths
        var expectedExpiration = result.CompletedAt.Value.AddMonths(validityMonths);
        Assert.Equal(expectedExpiration, capturedEvent.Payload.CertificationExpiration.Value);
    }

    [Fact]
    public async Task MarkCompletedAsync_NullExpiration_WhenValidityMonthsIsNull()
    {
        // Arrange
        await using var dbContext = CreateDbContext();

        var employeeId = Guid.NewGuid();
        var programId = Guid.NewGuid();

        var program = new TrainingProgram
        {
            Id = programId,
            ProgramName = "General Knowledge Training",
            ValidityMonths = null, // Never expires
            DurationHours = 2
        };
        dbContext.TrainingPrograms.Add(program);

        var enrollment = new EmployeeTrainingEnrollment
        {
            Id = Guid.NewGuid(),
            EmployeeId = employeeId,
            TrainingProgramId = programId,
            Status = TrainingEnrollmentStatus.InProgress,
            EnrolledAt = DateTime.UtcNow.AddDays(-1),
            EnrollmentType = EnrollmentType.Voluntary,
            RowVersion = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }
        };
        dbContext.EmployeeTrainingEnrollments.Add(enrollment);
        await dbContext.SaveChangesAsync();

        var service = new EnrollmentService(
            dbContext,
            _mockEmployeeClient.Object,
            _mockMetricsService.Object,
            _mockPublishEndpoint.Object,
            _mockLogger.Object);

        var request = new MarkTrainingCompleteRequest
        {
            CompletionNotes = "Attended",
            RowVersion = Convert.ToBase64String(enrollment.RowVersion)
        };

        TrainingCompletedEvent? capturedEvent = null;
        _mockPublishEndpoint
            .Setup(x => x.Publish(It.IsAny<TrainingCompletedEvent>(), It.IsAny<CancellationToken>()))
            .Callback<TrainingCompletedEvent, CancellationToken>((e, c) => capturedEvent = e)
            .Returns(Task.CompletedTask);

        // Act
        var result = await service.MarkCompletedAsync(enrollment.Id, request, Guid.NewGuid());

        // Assert
        Assert.NotNull(result);
        Assert.Null(capturedEvent?.Payload.CertificationExpiration);
    }
}
