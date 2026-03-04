using Maliev.CareerService.Api.BackgroundServices;
using Maliev.CareerService.Api.Services.External;
using Maliev.CareerService.Infrastructure.Data;
using Maliev.CareerService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Maliev.CareerService.Tests.Unit.BackgroundServices;

/// <summary>
/// Unit tests for CertificationExpirationReminderBackgroundService
/// </summary>
public class CertificationExpirationReminderTests : BaseUnitTests
{
    private readonly Mock<INotificationServiceClient> _mockNotificationClient;
    private readonly Mock<ILogger<CertificationExpirationReminderBackgroundService>> _mockLogger;
    private readonly Mock<IServiceScopeFactory> _mockScopeFactory;
    private readonly Mock<IServiceScope> _mockScope;
    private readonly Mock<IServiceProvider> _mockServiceProvider;

    public CertificationExpirationReminderTests()
    {
        _mockNotificationClient = new Mock<INotificationServiceClient>();
        _mockLogger = new Mock<ILogger<CertificationExpirationReminderBackgroundService>>();

        _mockScopeFactory = new Mock<IServiceScopeFactory>();
        _mockScope = new Mock<IServiceScope>();
        _mockServiceProvider = new Mock<IServiceProvider>();

        _mockScopeFactory.Setup(x => x.CreateScope()).Returns(_mockScope.Object);
        _mockScope.Setup(x => x.ServiceProvider).Returns(_mockServiceProvider.Object);
    }

    private void SetupServiceProvider(CareerDbContext dbContext)
    {
        _mockServiceProvider.Setup(x => x.GetService(typeof(CareerDbContext))).Returns(dbContext);
        _mockServiceProvider.Setup(x => x.GetService(typeof(INotificationServiceClient))).Returns(_mockNotificationClient.Object);
    }

    [Fact]
    public async Task ProcessExpirationsAsync_FindsRecordsExpiring30DaysOut()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        SetupServiceProvider(dbContext);

        var employeeId = Guid.NewGuid();
        var expirationDate = DateTime.UtcNow.AddDays(30);

        var record = new TrainingRecord
        {
            Id = Guid.NewGuid(),
            EmployeeId = employeeId,
            CourseName = "Safety Training",
            CompletionDate = DateTime.UtcNow.AddDays(-335),
            ExpirationDate = expirationDate,
            Status = TrainingStatus.Completed,
            TrainingType = TrainingType.InPerson,
            CreatedBy = Guid.NewGuid(),
            UpdatedBy = Guid.NewGuid()
        };

        dbContext.TrainingRecords.Add(record);
        await dbContext.SaveChangesAsync();

        var service = new CertificationExpirationReminderBackgroundService(
            _mockScopeFactory.Object,
            _mockLogger.Object);

        // Act
        await service.ProcessExpirationsAsync(CancellationToken.None);

        // Assert
        _mockNotificationClient.Verify(
            x => x.SendCertificationReminderAsync(
                employeeId,
                "Safety Training",
                It.IsAny<DateTime>(),
                30,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ProcessExpirationsAsync_FindsRecordsExpiring60DaysOut()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        SetupServiceProvider(dbContext);

        var employeeId = Guid.NewGuid();
        var expirationDate = DateTime.UtcNow.AddDays(60);

        var record = new TrainingRecord
        {
            Id = Guid.NewGuid(),
            EmployeeId = employeeId,
            CourseName = "First Aid",
            CompletionDate = DateTime.UtcNow.AddDays(-305),
            ExpirationDate = expirationDate,
            Status = TrainingStatus.Completed,
            TrainingType = TrainingType.InPerson,
            CreatedBy = Guid.NewGuid(),
            UpdatedBy = Guid.NewGuid()
        };

        dbContext.TrainingRecords.Add(record);
        await dbContext.SaveChangesAsync();

        var service = new CertificationExpirationReminderBackgroundService(
            _mockScopeFactory.Object,
            _mockLogger.Object);

        // Act
        await service.ProcessExpirationsAsync(CancellationToken.None);

        // Assert
        _mockNotificationClient.Verify(
            x => x.SendCertificationReminderAsync(
                employeeId,
                "First Aid",
                It.IsAny<DateTime>(),
                60,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ProcessExpirationsAsync_FindsRecordsExpiring90DaysOut()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        SetupServiceProvider(dbContext);

        var employeeId = Guid.NewGuid();
        var expirationDate = DateTime.UtcNow.AddDays(90);

        var record = new TrainingRecord
        {
            Id = Guid.NewGuid(),
            EmployeeId = employeeId,
            CourseName = "CPR Certification",
            CompletionDate = DateTime.UtcNow.AddDays(-275),
            ExpirationDate = expirationDate,
            Status = TrainingStatus.Completed,
            TrainingType = TrainingType.InPerson,
            CreatedBy = Guid.NewGuid(),
            UpdatedBy = Guid.NewGuid()
        };

        dbContext.TrainingRecords.Add(record);
        await dbContext.SaveChangesAsync();

        var service = new CertificationExpirationReminderBackgroundService(
            _mockScopeFactory.Object,
            _mockLogger.Object);

        // Act
        await service.ProcessExpirationsAsync(CancellationToken.None);

        // Assert
        _mockNotificationClient.Verify(
            x => x.SendCertificationReminderAsync(
                employeeId,
                "CPR Certification",
                It.IsAny<DateTime>(),
                90,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ProcessExpirationsAsync_IgnoresRecordsWithNoExpirationDate()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        SetupServiceProvider(dbContext);

        var record = new TrainingRecord
        {
            Id = Guid.NewGuid(),
            EmployeeId = Guid.NewGuid(),
            CourseName = "Non-Expiring Training",
            CompletionDate = DateTime.UtcNow.AddDays(-100),
            ExpirationDate = null,
            Status = TrainingStatus.Completed,
            TrainingType = TrainingType.Online,
            CreatedBy = Guid.NewGuid(),
            UpdatedBy = Guid.NewGuid()
        };

        dbContext.TrainingRecords.Add(record);
        await dbContext.SaveChangesAsync();

        var service = new CertificationExpirationReminderBackgroundService(
            _mockScopeFactory.Object,
            _mockLogger.Object);

        // Act
        await service.ProcessExpirationsAsync(CancellationToken.None);

        // Assert
        _mockNotificationClient.Verify(
            x => x.SendCertificationReminderAsync(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<DateTime>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ProcessExpirationsAsync_UpdatesStatusToExpiredWhenExpired()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        SetupServiceProvider(dbContext);

        var recordId = Guid.NewGuid();
        var record = new TrainingRecord
        {
            Id = recordId,
            EmployeeId = Guid.NewGuid(),
            CourseName = "Expired Training",
            CompletionDate = DateTime.UtcNow.AddDays(-400),
            ExpirationDate = DateTime.UtcNow.AddDays(-1),
            Status = TrainingStatus.Completed,
            TrainingType = TrainingType.InPerson,
            CreatedBy = Guid.NewGuid(),
            UpdatedBy = Guid.NewGuid()
        };

        dbContext.TrainingRecords.Add(record);
        await dbContext.SaveChangesAsync();

        var service = new CertificationExpirationReminderBackgroundService(
            _mockScopeFactory.Object,
            _mockLogger.Object);

        // Act
        await service.ProcessExpirationsAsync(CancellationToken.None);

        // Assert
        var updatedRecord = await dbContext.TrainingRecords.FindAsync(recordId);
        Assert.NotNull(updatedRecord);
        Assert.Equal(TrainingStatus.Expired, updatedRecord.Status);
    }

    [Fact]
    public async Task ProcessExpirationsAsync_IgnoresAlreadyExpiredRecords()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        SetupServiceProvider(dbContext);

        var record = new TrainingRecord
        {
            Id = Guid.NewGuid(),
            EmployeeId = Guid.NewGuid(),
            CourseName = "Already Expired",
            CompletionDate = DateTime.UtcNow.AddDays(-400),
            ExpirationDate = DateTime.UtcNow.AddDays(-10),
            Status = TrainingStatus.Expired,
            TrainingType = TrainingType.InPerson,
            CreatedBy = Guid.NewGuid(),
            UpdatedBy = Guid.NewGuid()
        };

        dbContext.TrainingRecords.Add(record);
        await dbContext.SaveChangesAsync();

        var service = new CertificationExpirationReminderBackgroundService(
            _mockScopeFactory.Object,
            _mockLogger.Object);

        // Act
        await service.ProcessExpirationsAsync(CancellationToken.None);

        // Assert - no notifications sent for already expired records
        _mockNotificationClient.Verify(
            x => x.SendCertificationReminderAsync(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<DateTime>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ProcessExpirationsAsync_HandlesNotificationFailureGracefully()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        SetupServiceProvider(dbContext);

        var employeeId = Guid.NewGuid();
        var record = new TrainingRecord
        {
            Id = Guid.NewGuid(),
            EmployeeId = employeeId,
            CourseName = "Test Training",
            CompletionDate = DateTime.UtcNow.AddDays(-335),
            ExpirationDate = DateTime.UtcNow.AddDays(30),
            Status = TrainingStatus.Completed,
            TrainingType = TrainingType.InPerson,
            CreatedBy = Guid.NewGuid(),
            UpdatedBy = Guid.NewGuid()
        };

        dbContext.TrainingRecords.Add(record);
        await dbContext.SaveChangesAsync();

        _mockNotificationClient
            .Setup(x => x.SendCertificationReminderAsync(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<DateTime>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Service unavailable"));

        var service = new CertificationExpirationReminderBackgroundService(
            _mockScopeFactory.Object,
            _mockLogger.Object);

        // Act & Assert - should not throw
        await service.ProcessExpirationsAsync(CancellationToken.None);

        // Verify error was logged
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.AtLeastOnce);
    }
}
