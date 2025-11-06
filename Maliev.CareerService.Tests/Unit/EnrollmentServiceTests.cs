using AutoMapper;
using FluentAssertions;
using Maliev.CareerService.Api.Models.Enrollments;
using Maliev.CareerService.Api.Services;
using Maliev.CareerService.Api.Services.External;
using Maliev.CareerService.Data;
using Maliev.CareerService.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Maliev.CareerService.Tests.Unit;

/// <summary>
/// Unit tests for EnrollmentService
/// </summary>
public class EnrollmentServiceTests : IDisposable
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IEmployeeServiceClient> _employeeServiceMock;
    private readonly Mock<IMetricsService> _metricsServiceMock;
    private readonly Mock<ILogger<EnrollmentService>> _loggerMock;
    private readonly CareerDbContext _dbContext;
    private readonly EnrollmentService _service;

    public EnrollmentServiceTests()
    {
        var options = new DbContextOptionsBuilder<CareerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new CareerDbContext(options);
        _mapperMock = new Mock<IMapper>();
        _employeeServiceMock = new Mock<IEmployeeServiceClient>();
        _metricsServiceMock = new Mock<IMetricsService>();
        _loggerMock = new Mock<ILogger<EnrollmentService>>();

        _service = new EnrollmentService(
            _dbContext,
            _mapperMock.Object,
            _employeeServiceMock.Object,
            _metricsServiceMock.Object,
            _loggerMock.Object
        );
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    #region EnrollEmployeeAsync Tests

    [Fact]
    public async Task EnrollEmployeeAsync_WithValidRequest_CreatesEnrollment()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var program = CreateTrainingProgram("TRN-001", isActive: true);
        await _dbContext.TrainingPrograms.AddAsync(program);
        await _dbContext.SaveChangesAsync();

        var request = new EnrollInTrainingRequest
        {
            TrainingProgramId = program.Id
        };

        _employeeServiceMock
            .Setup(x => x.ValidateEmployeeAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _mapperMock
            .Setup(x => x.Map<EmployeeTrainingEnrollment>(request))
            .Returns(new EmployeeTrainingEnrollment
            {
                Id = Guid.NewGuid(),
                TrainingProgramId = request.TrainingProgramId,
                Status = TrainingEnrollmentStatus.Enrolled
            });

        SetupMapperForEnrollment();

        // Act
        var result = await _service.EnrollEmployeeAsync(request, employeeId);

        // Assert
        result.Should().NotBeNull();

        var savedEnrollment = await _dbContext.EmployeeTrainingEnrollments.FirstOrDefaultAsync();
        savedEnrollment.Should().NotBeNull();
        savedEnrollment!.EmployeeId.Should().Be(employeeId);
        savedEnrollment.TrainingProgramId.Should().Be(program.Id);

        _metricsServiceMock.Verify(x => x.IncrementTrainingEnrollments(TrainingEnrollmentStatus.Enrolled), Times.Once);
    }

    [Fact]
    public async Task EnrollEmployeeAsync_WhenEmployeeNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var request = new EnrollInTrainingRequest
        {
            TrainingProgramId = Guid.NewGuid()
        };

        _employeeServiceMock
            .Setup(x => x.ValidateEmployeeAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act & Assert
        await _service.Invoking(s => s.EnrollEmployeeAsync(request, employeeId))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*{employeeId}*not found*");
    }

    [Fact]
    public async Task EnrollEmployeeAsync_WhenProgramNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var programId = Guid.NewGuid();

        var request = new EnrollInTrainingRequest
        {
            TrainingProgramId = programId
        };

        _employeeServiceMock
            .Setup(x => x.ValidateEmployeeAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act & Assert
        await _service.Invoking(s => s.EnrollEmployeeAsync(request, employeeId))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*{programId}*not found*");
    }

    [Fact]
    public async Task EnrollEmployeeAsync_WhenProgramNotActive_ThrowsInvalidOperationException()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var program = CreateTrainingProgram("TRN-001", isActive: false);
        await _dbContext.TrainingPrograms.AddAsync(program);
        await _dbContext.SaveChangesAsync();

        var request = new EnrollInTrainingRequest
        {
            TrainingProgramId = program.Id
        };

        _employeeServiceMock
            .Setup(x => x.ValidateEmployeeAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act & Assert
        await _service.Invoking(s => s.EnrollEmployeeAsync(request, employeeId))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*{program.Id}*not active*");
    }

    [Fact]
    public async Task EnrollEmployeeAsync_WhenDuplicateEnrollment_ThrowsInvalidOperationException()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var program = CreateTrainingProgram("TRN-001", isActive: true);
        await _dbContext.TrainingPrograms.AddAsync(program);

        var existingEnrollment = new EmployeeTrainingEnrollment
        {
            Id = Guid.NewGuid(),
            TrainingProgramId = program.Id,
            EmployeeId = employeeId,
            Status = TrainingEnrollmentStatus.Enrolled,
            EnrolledAt = DateTime.UtcNow,
            CreatedBy = employeeId,
            UpdatedBy = employeeId
        };
        await _dbContext.EmployeeTrainingEnrollments.AddAsync(existingEnrollment);
        await _dbContext.SaveChangesAsync();

        var request = new EnrollInTrainingRequest
        {
            TrainingProgramId = program.Id
        };

        _employeeServiceMock
            .Setup(x => x.ValidateEmployeeAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act & Assert
        await _service.Invoking(s => s.EnrollEmployeeAsync(request, employeeId))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*{employeeId}*already enrolled*");
    }

    [Fact]
    public async Task EnrollEmployeeAsync_WhenProgramAtCapacity_ThrowsInvalidOperationException()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var program = CreateTrainingProgram("TRN-001", isActive: true);
        program.MaxParticipants = 1;
        await _dbContext.TrainingPrograms.AddAsync(program);

        var existingEnrollment = new EmployeeTrainingEnrollment
        {
            Id = Guid.NewGuid(),
            TrainingProgramId = program.Id,
            EmployeeId = Guid.NewGuid(),
            Status = TrainingEnrollmentStatus.Enrolled,
            EnrolledAt = DateTime.UtcNow,
            CreatedBy = Guid.NewGuid(),
            UpdatedBy = Guid.NewGuid()
        };
        await _dbContext.EmployeeTrainingEnrollments.AddAsync(existingEnrollment);
        await _dbContext.SaveChangesAsync();

        var request = new EnrollInTrainingRequest
        {
            TrainingProgramId = program.Id
        };

        _employeeServiceMock
            .Setup(x => x.ValidateEmployeeAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act & Assert
        await _service.Invoking(s => s.EnrollEmployeeAsync(request, employeeId))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*{program.Id}*maximum capacity*");
    }

    [Fact]
    public async Task EnrollEmployeeAsync_WithMandatoryProgram_SetsEnrollmentTypeToMandatory()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var program = CreateTrainingProgram("TRN-001", isActive: true);
        program.IsMandatory = true;
        await _dbContext.TrainingPrograms.AddAsync(program);
        await _dbContext.SaveChangesAsync();

        var request = new EnrollInTrainingRequest
        {
            TrainingProgramId = program.Id
        };

        _employeeServiceMock
            .Setup(x => x.ValidateEmployeeAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _mapperMock
            .Setup(x => x.Map<EmployeeTrainingEnrollment>(request))
            .Returns(new EmployeeTrainingEnrollment
            {
                Id = Guid.NewGuid(),
                TrainingProgramId = request.TrainingProgramId,
                Status = TrainingEnrollmentStatus.Enrolled
            });

        SetupMapperForEnrollment();

        // Act
        await _service.EnrollEmployeeAsync(request, employeeId);

        // Assert
        var savedEnrollment = await _dbContext.EmployeeTrainingEnrollments.FirstOrDefaultAsync();
        savedEnrollment!.EnrollmentType.Should().Be(Data.Models.EnrollmentType.Mandatory);
    }

    #endregion

    #region GetEmployeeEnrollmentsAsync Tests

    [Fact]
    public async Task GetEmployeeEnrollmentsAsync_ReturnsEmployeeEnrollments()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var program = CreateTrainingProgram("TRN-001", isActive: true);
        await _dbContext.TrainingPrograms.AddAsync(program);

        var enrollment = CreateEnrollment(program.Id, employeeId, TrainingEnrollmentStatus.Enrolled);
        await _dbContext.EmployeeTrainingEnrollments.AddAsync(enrollment);
        await _dbContext.SaveChangesAsync();

        SetupMapperForEnrollment();

        // Act
        var result = await _service.GetEmployeeEnrollmentsAsync(employeeId, null, 1, 10);

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(1);
        result.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetEmployeeEnrollmentsAsync_WithStatusFilter_ReturnsFilteredEnrollments()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var program = CreateTrainingProgram("TRN-001", isActive: true);
        await _dbContext.TrainingPrograms.AddAsync(program);

        var enrollment1 = CreateEnrollment(program.Id, employeeId, TrainingEnrollmentStatus.Enrolled);
        var enrollment2 = CreateEnrollment(program.Id, employeeId, TrainingEnrollmentStatus.Completed);
        await _dbContext.EmployeeTrainingEnrollments.AddRangeAsync(enrollment1, enrollment2);
        await _dbContext.SaveChangesAsync();

        SetupMapperForEnrollment();

        // Act
        var result = await _service.GetEmployeeEnrollmentsAsync(employeeId, TrainingEnrollmentStatus.Completed, 1, 10);

        // Assert
        result.TotalCount.Should().Be(1);
        result.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetEmployeeEnrollmentsAsync_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var program = CreateTrainingProgram("TRN-001", isActive: true);
        await _dbContext.TrainingPrograms.AddAsync(program);

        for (int i = 0; i < 15; i++)
        {
            var enrollment = CreateEnrollment(program.Id, employeeId, TrainingEnrollmentStatus.Enrolled);
            await _dbContext.EmployeeTrainingEnrollments.AddAsync(enrollment);
        }
        await _dbContext.SaveChangesAsync();

        SetupMapperForEnrollment();

        // Act
        var result = await _service.GetEmployeeEnrollmentsAsync(employeeId, null, pageNumber: 2, pageSize: 5);

        // Assert
        result.TotalCount.Should().Be(15);
        result.Items.Should().HaveCount(5);
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(5);
        result.TotalPages.Should().Be(3);
    }

    #endregion

    #region MarkCompletedAsync Tests

    [Fact]
    public async Task MarkCompletedAsync_WhenExists_MarksAsCompleted()
    {
        // Arrange
        var program = CreateTrainingProgram("TRN-001", isActive: true);
        await _dbContext.TrainingPrograms.AddAsync(program);

        var enrollment = CreateEnrollment(program.Id, Guid.NewGuid(), TrainingEnrollmentStatus.Enrolled);
        await _dbContext.EmployeeTrainingEnrollments.AddAsync(enrollment);
        await _dbContext.SaveChangesAsync();

        // Detach and reload
        _dbContext.Entry(enrollment).State = EntityState.Detached;
        var freshEnrollment = await _dbContext.EmployeeTrainingEnrollments.FirstAsync(e => e.Id == enrollment.Id);

        var markedBy = Guid.NewGuid();
        var request = new MarkTrainingCompleteRequest
        {
            CompletionNotes = "Successfully completed",
            RowVersion = Convert.ToBase64String(freshEnrollment.RowVersion)
        };

        SetupMapperForEnrollment();

        // Act
        var result = await _service.MarkCompletedAsync(enrollment.Id, request, markedBy);

        // Assert
        result.Should().NotBeNull();

        var updated = await _dbContext.EmployeeTrainingEnrollments.FirstAsync(e => e.Id == enrollment.Id);
        updated.Status.Should().Be(TrainingEnrollmentStatus.Completed);
        updated.CompletedAt.Should().NotBeNull();
        updated.CompletionNotes.Should().Be("Successfully completed");
        updated.MarkedCompleteBy.Should().Be(markedBy);

        _metricsServiceMock.Verify(x => x.IncrementTrainingEnrollments(TrainingEnrollmentStatus.Completed), Times.Once);
    }

    [Fact]
    public async Task MarkCompletedAsync_WhenNotExists_ReturnsNull()
    {
        // Arrange
        var request = new MarkTrainingCompleteRequest
        {
            CompletionNotes = "Completed",
            RowVersion = Convert.ToBase64String(new byte[8])
        };

        // Act
        var result = await _service.MarkCompletedAsync(Guid.NewGuid(), request, Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region ValidateCapacityAsync Tests

    [Fact]
    public async Task ValidateCapacityAsync_WhenProgramNotFound_ReturnsFalse()
    {
        // Act
        var result = await _service.ValidateCapacityAsync(Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateCapacityAsync_WhenNoCapacityLimit_ReturnsTrue()
    {
        // Arrange
        var program = CreateTrainingProgram("TRN-001", isActive: true);
        program.MaxParticipants = null;
        await _dbContext.TrainingPrograms.AddAsync(program);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.ValidateCapacityAsync(program.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateCapacityAsync_WhenBelowCapacity_ReturnsTrue()
    {
        // Arrange
        var program = CreateTrainingProgram("TRN-001", isActive: true);
        program.MaxParticipants = 10;
        await _dbContext.TrainingPrograms.AddAsync(program);

        for (int i = 0; i < 5; i++)
        {
            var enrollment = CreateEnrollment(program.Id, Guid.NewGuid(), TrainingEnrollmentStatus.Enrolled);
            await _dbContext.EmployeeTrainingEnrollments.AddAsync(enrollment);
        }
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.ValidateCapacityAsync(program.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateCapacityAsync_WhenAtCapacity_ReturnsFalse()
    {
        // Arrange
        var program = CreateTrainingProgram("TRN-001", isActive: true);
        program.MaxParticipants = 5;
        await _dbContext.TrainingPrograms.AddAsync(program);

        for (int i = 0; i < 5; i++)
        {
            var enrollment = CreateEnrollment(program.Id, Guid.NewGuid(), TrainingEnrollmentStatus.Enrolled);
            await _dbContext.EmployeeTrainingEnrollments.AddAsync(enrollment);
        }
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.ValidateCapacityAsync(program.Id);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateCapacityAsync_ExcludesCancelledEnrollments()
    {
        // Arrange
        var program = CreateTrainingProgram("TRN-001", isActive: true);
        program.MaxParticipants = 5;
        await _dbContext.TrainingPrograms.AddAsync(program);

        for (int i = 0; i < 3; i++)
        {
            var enrollment = CreateEnrollment(program.Id, Guid.NewGuid(), TrainingEnrollmentStatus.Enrolled);
            await _dbContext.EmployeeTrainingEnrollments.AddAsync(enrollment);
        }

        for (int i = 0; i < 5; i++)
        {
            var enrollment = CreateEnrollment(program.Id, Guid.NewGuid(), TrainingEnrollmentStatus.Cancelled);
            await _dbContext.EmployeeTrainingEnrollments.AddAsync(enrollment);
        }
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.ValidateCapacityAsync(program.Id);

        // Assert - Only 3 active (not cancelled) enrollments, capacity is 5
        result.Should().BeTrue();
    }

    #endregion

    #region CheckDuplicateEnrollmentAsync Tests

    [Fact]
    public async Task CheckDuplicateEnrollmentAsync_WhenExists_ReturnsTrue()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var program = CreateTrainingProgram("TRN-001", isActive: true);
        await _dbContext.TrainingPrograms.AddAsync(program);

        var enrollment = CreateEnrollment(program.Id, employeeId, TrainingEnrollmentStatus.Enrolled);
        await _dbContext.EmployeeTrainingEnrollments.AddAsync(enrollment);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.CheckDuplicateEnrollmentAsync(program.Id, employeeId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CheckDuplicateEnrollmentAsync_WhenNotExists_ReturnsFalse()
    {
        // Act
        var result = await _service.CheckDuplicateEnrollmentAsync(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Helper Methods

    private TrainingProgram CreateTrainingProgram(string programCode, bool isActive)
    {
        return new TrainingProgram
        {
            Id = Guid.NewGuid(),
            ProgramCode = programCode,
            ProgramName = $"Program {programCode}",
            Description = "Training program description",
            DurationHours = 40,
            IsActive = isActive,
            IsMandatory = false,
            CreatedBy = Guid.NewGuid(),
            UpdatedBy = Guid.NewGuid()
        };
    }

    private EmployeeTrainingEnrollment CreateEnrollment(Guid programId, Guid employeeId, string status)
    {
        return new EmployeeTrainingEnrollment
        {
            Id = Guid.NewGuid(),
            TrainingProgramId = programId,
            EmployeeId = employeeId,
            Status = status,
            EnrolledAt = DateTime.UtcNow,
            CreatedBy = employeeId,
            UpdatedBy = employeeId
        };
    }

    private void SetupMapperForEnrollment()
    {
        _mapperMock
            .Setup(x => x.Map<TrainingEnrollmentResponse>(It.IsAny<EmployeeTrainingEnrollment>()))
            .Returns((EmployeeTrainingEnrollment e) => new TrainingEnrollmentResponse
            {
                Id = e.Id,
                TrainingProgramId = e.TrainingProgramId,
                EmployeeId = e.EmployeeId,
                Status = e.Status,
                EnrolledAt = e.EnrolledAt,
                CompletedAt = e.CompletedAt,
                CompletionNotes = e.CompletionNotes
            });

        _mapperMock
            .Setup(x => x.Map<Api.Models.TrainingPrograms.TrainingProgramResponse>(It.IsAny<TrainingProgram>()))
            .Returns((TrainingProgram tp) => new Api.Models.TrainingPrograms.TrainingProgramResponse
            {
                Id = tp.Id,
                ProgramCode = tp.ProgramCode,
                ProgramName = tp.ProgramName
            });
    }

    #endregion
}
