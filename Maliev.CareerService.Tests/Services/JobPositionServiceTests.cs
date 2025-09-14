using FluentAssertions;
using Maliev.CareerService.Api.Models;
using Maliev.CareerService.Api.Services;
using Maliev.CareerService.Data.DbContexts;
using Maliev.CareerService.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Maliev.CareerService.Tests.Services;

public class JobPositionServiceTests : IDisposable
{
    private readonly CareerDbContext _context;
    private readonly Mock<ILogger<JobPositionService>> _mockLogger;
    private readonly IMemoryCache _cache;
    private readonly CacheOptions _cacheOptions;
    private readonly JobPositionService _jobPositionService;

    public JobPositionServiceTests()
    {
        var options = new DbContextOptionsBuilder<CareerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new CareerDbContext(options);
        _mockLogger = new Mock<ILogger<JobPositionService>>();
        _cache = new MemoryCache(new MemoryCacheOptions());
        _cacheOptions = new CacheOptions 
        { 
            MaxCacheSize = 100,
            DefaultExpiration = TimeSpan.FromMinutes(30),
            LongExpiration = TimeSpan.FromHours(2)
        };

        _jobPositionService = new JobPositionService(
            _context,
            _cache,
            _mockLogger.Object,
            _cacheOptions);

        SeedTestData();
    }

    private void SeedTestData()
    {
        var workLocation = new WorkLocation
        {
            Id = 1,
            Name = "Bangkok Office",
            Address = "123 Test Street",
            City = "Bangkok",
            CountryId = 1,
            IsActive = true
        };

        var skill = new Skill
        {
            Id = 1,
            Name = ".NET Core",
            Category = "Programming",
            IsActive = true
        };

        var jobPosition = new JobPosition
        {
            Id = 1,
            Title = "Software Engineer",
            Department = "Engineering",
            Description = "Develop software applications",
            EmploymentType = "Full-time",
            ExperienceLevel = "Mid-level",
            SalaryRangeMin = 50000,
            SalaryRangeMax = 80000,
            Currency = "THB",
            IsActive = true,
            IsPublic = true
        };

        _context.WorkLocations.Add(workLocation);
        _context.Skills.Add(skill);
        _context.JobPositions.Add(jobPosition);

        var jobPositionLocation = new JobPositionLocation
        {
            JobPositionId = 1,
            WorkLocationId = 1
        };

        var jobPositionSkill = new JobPositionSkill
        {
            JobPositionId = 1,
            SkillId = 1,
            IsRequired = true,
            RequiredLevel = "Intermediate"
        };

        _context.JobPositionLocations.Add(jobPositionLocation);
        _context.JobPositionSkills.Add(jobPositionSkill);

        _context.SaveChanges();
    }

    [Fact]
    public async Task GetByIdAsync_ExistingJobPosition_ReturnsJobPositionDto()
    {
        // Arrange

        // Act
        var result = await _jobPositionService.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Title.Should().Be("Software Engineer");
        result.Department.Should().Be("Engineering");
        result.EmploymentType.Should().Be("Full-time");
        result.ExperienceLevel.Should().Be("Mid-level");
        result.SalaryRangeMin.Should().Be(50000);
        result.SalaryRangeMax.Should().Be(80000);
        result.Currency.Should().Be("THB");
        result.IsActive.Should().BeTrue();
        result.IsPublic.Should().BeTrue();
        result.WorkLocations.Should().HaveCount(1);
        result.Skills.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingJobPosition_ReturnsNull()
    {
        // Arrange

        // Act
        var result = await _jobPositionService.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_CachedJobPosition_ReturnsCachedResult()
    {
        // Arrange
        var cachedDto = new JobPositionDto
        {
            Id = 1,
            Title = "Cached Position",
            Department = "Cached Department",
            Description = "Cached Description",
            EmploymentType = "Full-time",
            ExperienceLevel = "Senior",
            IsActive = true,
            IsPublic = true,
            WorkLocations = new List<WorkLocationDto>(),
            Skills = new List<JobPositionSkillDto>(),
            ApplicationCount = 0
        };

        _cache.Set("jobposition_1", cachedDto);

        // Act
        var result = await _jobPositionService.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be("Cached Position");
        result.Department.Should().Be("Cached Department");
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsCreatedJobPosition()
    {
        // Arrange
        var request = new CreateJobPositionRequest
        {
            Title = "Senior Developer",
            Department = "Engineering",
            Description = "Lead development team",
            Requirements = "5+ years experience",
            Responsibilities = "Code review, mentoring",
            EmploymentType = "Full-time",
            ExperienceLevel = "Senior",
            SalaryRangeMin = 80000,
            SalaryRangeMax = 120000,
            Currency = "THB",
            WorkLocationIds = new List<int> { 1 },
            Skills = new List<CreateJobPositionSkillRequest>
            {
                new() { SkillId = 1, IsRequired = true, RequiredLevel = "Advanced" }
            }
        };

        // Act
        var result = await _jobPositionService.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("Senior Developer");
        result.Department.Should().Be("Engineering");
        result.SalaryRangeMin.Should().Be(80000);
        result.SalaryRangeMax.Should().Be(120000);
        result.WorkLocations.Should().HaveCount(1);
        result.Skills.Should().HaveCount(1);

        // Verify in database
        var positionInDb = await _context.JobPositions
            .Include(jp => jp.JobPositionLocations)
            .Include(jp => jp.JobPositionSkills)
            .FirstAsync(jp => jp.Id == result.Id);
        
        positionInDb.Title.Should().Be("Senior Developer");
        positionInDb.JobPositionLocations.Should().HaveCount(1);
        positionInDb.JobPositionSkills.Should().HaveCount(1);
    }

    [Fact]
    public async Task UpdateAsync_ExistingPosition_ReturnsUpdatedJobPosition()
    {
        // Arrange
        var request = new UpdateJobPositionRequest
        {
            Title = "Updated Software Engineer",
            Department = "Updated Engineering",
            Description = "Updated description",
            EmploymentType = "Part-time",
            ExperienceLevel = "Senior",
            SalaryRangeMin = 60000,
            SalaryRangeMax = 90000
        };

        // Act
        var result = await _jobPositionService.UpdateAsync(1, request);

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be("Updated Software Engineer");
        result.Department.Should().Be("Updated Engineering");
        result.EmploymentType.Should().Be("Part-time");
        result.ExperienceLevel.Should().Be("Senior");
        result.SalaryRangeMin.Should().Be(60000);

        // Verify cache was cleared
        _cache.TryGetValue("jobposition_1", out _).Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_NonExistingPosition_ReturnsNull()
    {
        // Arrange
        var request = new UpdateJobPositionRequest
        {
            Title = "Non-existing Position",
            Department = "Department",
            Description = "Description",
            EmploymentType = "Full-time",
            ExperienceLevel = "Entry"
        };

        // Act
        var result = await _jobPositionService.UpdateAsync(999, request);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_ExistingPosition_ReturnsTrue()
    {
        // Act
        var result = await _jobPositionService.DeleteAsync(1);

        // Assert
        result.Should().BeTrue();

        // Verify position is marked as inactive
        var positionInDb = await _context.JobPositions.FirstOrDefaultAsync(jp => jp.Id == 1);
        positionInDb.Should().NotBeNull();
        positionInDb!.IsActive.Should().BeFalse();

        // Verify cache was cleared
        _cache.TryGetValue("jobposition_1", out _).Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_NonExistingPosition_ReturnsFalse()
    {
        // Act
        var result = await _jobPositionService.DeleteAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task SearchAsync_WithFilters_ReturnsFilteredResults()
    {
        // Arrange
        var additionalPosition = new JobPosition
        {
            Id = 3,
            Title = "Data Scientist",
            Department = "Analytics",
            Description = "Analyze data",
            EmploymentType = "Contract",
            ExperienceLevel = "Senior",
            IsActive = true,
            IsPublic = true
        };

        _context.JobPositions.Add(additionalPosition);
        await _context.SaveChangesAsync();

        var searchRequest = new JobPositionSearchRequest
        {
            Department = "Engineering",
            EmploymentType = "Full-time",
            IsActive = true
        };

        // Act
        var result = await _jobPositionService.SearchAsync(searchRequest);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items.First().Department.Should().Be("Engineering");
        result.Items.First().EmploymentType.Should().Be("Full-time");
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task GetPublicPositionsAsync_ReturnsOnlyPublicPositions()
    {
        // Arrange
        var privatePosition = new JobPosition
        {
            Id = 2,
            Title = "Private Position",
            Department = "HR",
            Description = "Internal only",
            EmploymentType = "Full-time",
            ExperienceLevel = "Entry",
            IsActive = true,
            IsPublic = false // Not public
        };

        _context.JobPositions.Add(privatePosition);
        await _context.SaveChangesAsync();

        var searchRequest = new JobPositionSearchRequest();

        // Act
        var result = await _jobPositionService.GetPublicPositionsAsync(searchRequest);

        // Assert
        var positions = result.Items.ToList();
        positions.Should().HaveCount(1);
        positions[0].IsPublic.Should().BeTrue();
        positions[0].Title.Should().Be("Software Engineer");
    }

    [Fact]
    public async Task ExistsAsync_ExistingPosition_ReturnsTrue()
    {
        // Act
        var result = await _jobPositionService.ExistsAsync(1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_NonExistingPosition_ReturnsFalse()
    {
        // Act
        var result = await _jobPositionService.ExistsAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsByTitleAndDepartmentAsync_ExistingPosition_ReturnsTrue()
    {
        // Act
        var result = await _jobPositionService.ExistsByTitleAndDepartmentAsync("Software Engineer", "Engineering");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByTitleAndDepartmentAsync_NonExistingPosition_ReturnsFalse()
    {
        // Act
        var result = await _jobPositionService.ExistsByTitleAndDepartmentAsync("Non-existing Position", "Non-existing Department");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetDepartmentsAsync_ReturnsUniqueDepartments()
    {
        // Arrange
        // No cache setup needed

        // Act
        var result = await _jobPositionService.GetDepartmentsAsync();

        // Assert
        var departments = result.ToList();
        departments.Should().HaveCount(1);
        departments.Should().Contain("Engineering");
    }

    public void Dispose()
    {
        _context.Dispose();
        _cache.Dispose();
    }
}