using AutoMapper;
using FluentAssertions;
using Maliev.CareerService.Api.Models.TrainingPrograms;
using Maliev.CareerService.Api.Services;
using Maliev.CareerService.Data;
using Maliev.CareerService.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Maliev.CareerService.Tests.Unit;

/// <summary>
/// Unit tests for TrainingProgramService
/// </summary>
public class TrainingProgramServiceTests : IDisposable
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<TrainingProgramService>> _loggerMock;
    private readonly CareerDbContext _dbContext;
    private readonly TrainingProgramService _service;

    public TrainingProgramServiceTests()
    {
        var options = new DbContextOptionsBuilder<CareerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new CareerDbContext(options);
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<TrainingProgramService>>();

        _service = new TrainingProgramService(
            _dbContext,
            _mapperMock.Object,
            _loggerMock.Object
        );
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    #region GetActiveProgramsAsync Tests

    [Fact]
    public async Task GetActiveProgramsAsync_ReturnsOnlyActivePrograms()
    {
        // Arrange
        var activeProgram = CreateTrainingProgram("TRN-001", isActive: true);
        var inactiveProgram = CreateTrainingProgram("TRN-002", isActive: false);

        await _dbContext.TrainingPrograms.AddRangeAsync(activeProgram, inactiveProgram);
        await _dbContext.SaveChangesAsync();

        SetupMapperForProgram();

        // Act
        var result = await _service.GetActiveProgramsAsync(1, 10);

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(1);
        result.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetActiveProgramsAsync_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        for (int i = 1; i <= 15; i++)
        {
            var program = CreateTrainingProgram($"TRN-{i:D3}", isActive: true);
            await _dbContext.TrainingPrograms.AddAsync(program);
        }
        await _dbContext.SaveChangesAsync();

        SetupMapperForProgram();

        // Act
        var result = await _service.GetActiveProgramsAsync(pageNumber: 2, pageSize: 5);

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(15);
        result.Items.Should().HaveCount(5);
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(5);
        result.TotalPages.Should().Be(3);
    }

    [Fact]
    public async Task GetActiveProgramsAsync_OrdersByProgramName()
    {
        // Arrange
        var program1 = CreateTrainingProgram("TRN-001", isActive: true);
        program1.ProgramName = "C Programming";

        var program2 = CreateTrainingProgram("TRN-002", isActive: true);
        program2.ProgramName = "A Leadership";

        var program3 = CreateTrainingProgram("TRN-003", isActive: true);
        program3.ProgramName = "B Technical";

        await _dbContext.TrainingPrograms.AddRangeAsync(program1, program2, program3);
        await _dbContext.SaveChangesAsync();

        _mapperMock
            .Setup(x => x.Map<List<TrainingProgramResponse>>(It.IsAny<List<TrainingProgram>>()))
            .Returns((List<TrainingProgram> programs) => programs.Select(p => new TrainingProgramResponse
            {
                Id = p.Id,
                ProgramCode = p.ProgramCode,
                ProgramName = p.ProgramName
            }).ToList());

        // Act
        var result = await _service.GetActiveProgramsAsync(1, 10);

        // Assert
        result.Items[0].ProgramName.Should().Be("A Leadership");
        result.Items[1].ProgramName.Should().Be("B Technical");
        result.Items[2].ProgramName.Should().Be("C Programming");
    }

    #endregion

    #region GetProgramByIdAsync Tests

    [Fact]
    public async Task GetProgramByIdAsync_WhenExists_ReturnsProgram()
    {
        // Arrange
        var program = CreateTrainingProgram("TRN-001", isActive: true);
        await _dbContext.TrainingPrograms.AddAsync(program);
        await _dbContext.SaveChangesAsync();

        SetupMapperForProgram();

        // Act
        var result = await _service.GetProgramByIdAsync(program.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(program.Id);
    }

    [Fact]
    public async Task GetProgramByIdAsync_WhenNotExists_ReturnsNull()
    {
        // Act
        var result = await _service.GetProgramByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region FilterProgramsAsync Tests

    [Fact]
    public async Task FilterProgramsAsync_WithCategoryFilter_ReturnsMatchingPrograms()
    {
        // Arrange
        var program1 = CreateTrainingProgram("TRN-001", isActive: true);
        program1.Category = "Technical";

        var program2 = CreateTrainingProgram("TRN-002", isActive: true);
        program2.Category = "Leadership";

        var program3 = CreateTrainingProgram("TRN-003", isActive: true);
        program3.Category = "Technical";

        await _dbContext.TrainingPrograms.AddRangeAsync(program1, program2, program3);
        await _dbContext.SaveChangesAsync();

        SetupMapperForProgram();

        // Act
        var result = await _service.FilterProgramsAsync(
            category: "Technical",
            isMandatory: null,
            pageNumber: 1,
            pageSize: 10);

        // Assert
        result.TotalCount.Should().Be(2);
        result.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task FilterProgramsAsync_WithMandatoryFilter_ReturnsMatchingPrograms()
    {
        // Arrange
        var program1 = CreateTrainingProgram("TRN-001", isActive: true);
        program1.IsMandatory = true;

        var program2 = CreateTrainingProgram("TRN-002", isActive: true);
        program2.IsMandatory = false;

        var program3 = CreateTrainingProgram("TRN-003", isActive: true);
        program3.IsMandatory = true;

        await _dbContext.TrainingPrograms.AddRangeAsync(program1, program2, program3);
        await _dbContext.SaveChangesAsync();

        SetupMapperForProgram();

        // Act
        var result = await _service.FilterProgramsAsync(
            category: null,
            isMandatory: true,
            pageNumber: 1,
            pageSize: 10);

        // Assert
        result.TotalCount.Should().Be(2);
        result.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task FilterProgramsAsync_WithBothFilters_CombinesCorrectly()
    {
        // Arrange
        var program1 = CreateTrainingProgram("TRN-001", isActive: true);
        program1.Category = "Technical";
        program1.IsMandatory = true;

        var program2 = CreateTrainingProgram("TRN-002", isActive: true);
        program2.Category = "Technical";
        program2.IsMandatory = false;

        var program3 = CreateTrainingProgram("TRN-003", isActive: true);
        program3.Category = "Leadership";
        program3.IsMandatory = true;

        await _dbContext.TrainingPrograms.AddRangeAsync(program1, program2, program3);
        await _dbContext.SaveChangesAsync();

        SetupMapperForProgram();

        // Act
        var result = await _service.FilterProgramsAsync(
            category: "Technical",
            isMandatory: true,
            pageNumber: 1,
            pageSize: 10);

        // Assert
        result.TotalCount.Should().Be(1);
        result.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task FilterProgramsAsync_WithNoFilters_ReturnsAllActivePrograms()
    {
        // Arrange
        var program1 = CreateTrainingProgram("TRN-001", isActive: true);
        var program2 = CreateTrainingProgram("TRN-002", isActive: true);
        var program3 = CreateTrainingProgram("TRN-003", isActive: false);

        await _dbContext.TrainingPrograms.AddRangeAsync(program1, program2, program3);
        await _dbContext.SaveChangesAsync();

        SetupMapperForProgram();

        // Act
        var result = await _service.FilterProgramsAsync(
            category: null,
            isMandatory: null,
            pageNumber: 1,
            pageSize: 10);

        // Assert
        result.TotalCount.Should().Be(2);
        result.Items.Should().HaveCount(2);
    }

    #endregion

    #region CreateProgramAsync Tests

    [Fact]
    public async Task CreateProgramAsync_WithValidRequest_CreatesProgram()
    {
        // Arrange
        var createdBy = Guid.NewGuid();
        var request = new CreateTrainingProgramRequest
        {
            ProgramCode = "TRN-001",
            ProgramName = "Leadership Training",
            Description = "Leadership program",
            DurationHours = 40
        };

        _mapperMock
            .Setup(x => x.Map<TrainingProgram>(request))
            .Returns(new TrainingProgram
            {
                Id = Guid.NewGuid(),
                ProgramCode = request.ProgramCode,
                ProgramName = request.ProgramName,
                Description = request.Description,
                DurationHours = request.DurationHours,
                IsActive = true
            });

        SetupMapperForProgram();

        // Act
        var result = await _service.CreateProgramAsync(request, createdBy);

        // Assert
        result.Should().NotBeNull();

        var savedProgram = await _dbContext.TrainingPrograms.FirstOrDefaultAsync();
        savedProgram.Should().NotBeNull();
        savedProgram!.ProgramCode.Should().Be("TRN-001");
        savedProgram.CreatedBy.Should().Be(createdBy);
        savedProgram.UpdatedBy.Should().Be(createdBy);
    }

    [Fact]
    public async Task CreateProgramAsync_WithDuplicateCode_ThrowsInvalidOperationException()
    {
        // Arrange
        var existingProgram = CreateTrainingProgram("TRN-001", isActive: true);
        await _dbContext.TrainingPrograms.AddAsync(existingProgram);
        await _dbContext.SaveChangesAsync();

        var request = new CreateTrainingProgramRequest
        {
            ProgramCode = "TRN-001",
            ProgramName = "Another Program",
            Description = "Description",
            DurationHours = 20
        };

        // Act & Assert
        await _service.Invoking(s => s.CreateProgramAsync(request, Guid.NewGuid()))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*TRN-001*already exists*");
    }

    #endregion

    #region UpdateProgramAsync Tests

    [Fact]
    public async Task UpdateProgramAsync_WhenExists_UpdatesProgram()
    {
        // Arrange
        var program = CreateTrainingProgram("TRN-001", isActive: true);
        await _dbContext.TrainingPrograms.AddAsync(program);
        await _dbContext.SaveChangesAsync();

        // Detach and reload to get fresh RowVersion
        _dbContext.Entry(program).State = EntityState.Detached;
        var freshProgram = await _dbContext.TrainingPrograms.FirstAsync(p => p.Id == program.Id);

        var updatedBy = Guid.NewGuid();
        var request = new UpdateTrainingProgramRequest
        {
            ProgramName = "Updated Leadership Training",
            Description = "Updated description",
            DurationHours = 50,
            RowVersion = Convert.ToBase64String(freshProgram.RowVersion)
        };

        _mapperMock
            .Setup(x => x.Map(request, It.IsAny<TrainingProgram>()))
            .Callback<UpdateTrainingProgramRequest, TrainingProgram>((req, prog) =>
            {
                prog.ProgramName = req.ProgramName;
                prog.Description = req.Description;
                prog.DurationHours = req.DurationHours;
            });

        SetupMapperForProgram();

        // Act
        var result = await _service.UpdateProgramAsync(program.Id, request, updatedBy);

        // Assert
        result.Should().NotBeNull();

        var updatedProgram = await _dbContext.TrainingPrograms.FirstAsync(p => p.Id == program.Id);
        updatedProgram.ProgramName.Should().Be("Updated Leadership Training");
        updatedProgram.UpdatedBy.Should().Be(updatedBy);
    }

    [Fact]
    public async Task UpdateProgramAsync_WhenNotExists_ReturnsNull()
    {
        // Arrange
        var request = new UpdateTrainingProgramRequest
        {
            ProgramName = "New Name",
            Description = "Description",
            DurationHours = 30,
            RowVersion = Convert.ToBase64String(new byte[8])
        };

        // Act
        var result = await _service.UpdateProgramAsync(Guid.NewGuid(), request, Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateProgramAsync_WithStaleRowVersion_ThrowsDbUpdateConcurrencyException()
    {
        // Arrange
        var program = CreateTrainingProgram("TRN-001", isActive: true);
        await _dbContext.TrainingPrograms.AddAsync(program);
        await _dbContext.SaveChangesAsync();

        var staleRowVersion = Convert.ToBase64String(new byte[] { 0, 0, 0, 0, 0, 0, 0, 1 });

        var request = new UpdateTrainingProgramRequest
        {
            ProgramName = "Updated Name",
            Description = "Description",
            DurationHours = 40,
            RowVersion = staleRowVersion
        };

        _mapperMock
            .Setup(x => x.Map(request, It.IsAny<TrainingProgram>()))
            .Callback<UpdateTrainingProgramRequest, TrainingProgram>((req, prog) =>
            {
                prog.ProgramName = req.ProgramName;
            });

        // Act & Assert
        await _service.Invoking(s => s.UpdateProgramAsync(program.Id, request, Guid.NewGuid()))
            .Should().ThrowAsync<DbUpdateConcurrencyException>()
            .WithMessage("*modified by another user*");
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
            Category = "Technical",
            DurationHours = 40,
            Provider = "Training Provider",
            IsActive = isActive,
            IsMandatory = false,
            CreatedBy = Guid.NewGuid(),
            UpdatedBy = Guid.NewGuid()
        };
    }

    private void SetupMapperForProgram()
    {
        _mapperMock
            .Setup(x => x.Map<TrainingProgramResponse>(It.IsAny<TrainingProgram>()))
            .Returns((TrainingProgram tp) => new TrainingProgramResponse
            {
                Id = tp.Id,
                ProgramCode = tp.ProgramCode,
                ProgramName = tp.ProgramName,
                Description = tp.Description,
                Category = tp.Category,
                DurationHours = tp.DurationHours,
                Provider = tp.Provider,
                IsActive = tp.IsActive,
                IsMandatory = tp.IsMandatory
            });

        _mapperMock
            .Setup(x => x.Map<List<TrainingProgramResponse>>(It.IsAny<List<TrainingProgram>>()))
            .Returns((List<TrainingProgram> programs) => programs.Select(tp => new TrainingProgramResponse
            {
                Id = tp.Id,
                ProgramCode = tp.ProgramCode,
                ProgramName = tp.ProgramName,
                Description = tp.Description,
                Category = tp.Category,
                DurationHours = tp.DurationHours,
                Provider = tp.Provider,
                IsActive = tp.IsActive,
                IsMandatory = tp.IsMandatory
            }).ToList());
    }

    #endregion
}
