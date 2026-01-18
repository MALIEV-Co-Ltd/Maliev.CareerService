using Maliev.CareerService.Api.Models.TrainingPrograms;
using Maliev.CareerService.Api.Services;
using Maliev.CareerService.Data.Models;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Maliev.CareerService.Tests.Unit.Services;

public class TrainingProgramServiceTests : BaseUnitTests
{
    private readonly Mock<ILogger<TrainingProgramService>> _mockLogger = new();

    [Fact]
    public async Task CreateProgramAsync_ShouldPersistProgram()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var service = new TrainingProgramService(dbContext, _mockLogger.Object);
        var request = new CreateTrainingProgramRequest
        {
            ProgramCode = "TP-001",
            ProgramName = "New Training",
            Description = "Test Description",
            DurationHours = 10,
            ValidityMonths = 24
        };

        // Act
        var result = await service.CreateProgramAsync(request, Guid.NewGuid());

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New Training", result.ProgramName);
        var dbProgram = await dbContext.TrainingPrograms.FindAsync(result.Id);
        Assert.NotNull(dbProgram);
    }

    [Fact]
    public async Task GetActiveProgramsAsync_ShouldReturnAllPrograms()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        dbContext.TrainingPrograms.Add(new TrainingProgram { Id = Guid.NewGuid(), ProgramCode = "P1", ProgramName = "P1", DurationHours = 1, IsActive = true });
        dbContext.TrainingPrograms.Add(new TrainingProgram { Id = Guid.NewGuid(), ProgramCode = "P2", ProgramName = "P2", DurationHours = 2, IsActive = true });
        await dbContext.SaveChangesAsync();

        var service = new TrainingProgramService(dbContext, _mockLogger.Object);

        // Act
        var result = await service.GetActiveProgramsAsync(1, 10);

        // Assert
        Assert.Equal(2, result.TotalCount);
    }
}
