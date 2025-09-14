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

public class SkillServiceTests : IDisposable
{
    private readonly CareerDbContext _context;
    private readonly Mock<ILogger<SkillService>> _mockLogger;
    private readonly IMemoryCache _cache;
    private readonly CacheOptions _cacheOptions;
    private readonly SkillService _skillService;

    public SkillServiceTests()
    {
        var options = new DbContextOptionsBuilder<CareerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new CareerDbContext(options);
        _mockLogger = new Mock<ILogger<SkillService>>();
        _cache = new MemoryCache(new MemoryCacheOptions());
        _cacheOptions = new CacheOptions 
        { 
            MaxCacheSize = 100,
            DefaultExpiration = TimeSpan.FromMinutes(30),
            LongExpiration = TimeSpan.FromHours(2)
        };

        _skillService = new SkillService(
            _context,
            _cache,
            _mockLogger.Object,
            _cacheOptions);

        SeedTestData();
    }

    private void SeedTestData()
    {
        var skills = new[]
        {
            new Skill
            {
                Id = 1,
                Name = ".NET Core",
                Category = "Programming Languages",
                IsActive = true
            },
            new Skill
            {
                Id = 2,
                Name = "React",
                Category = "Frontend Frameworks",
                IsActive = true
            },
            new Skill
            {
                Id = 3,
                Name = "Obsolete Technology",
                Category = "Legacy",
                IsActive = false
            }
        };

        _context.Skills.AddRange(skills);
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetByIdAsync_ExistingSkill_ReturnsSkillDto()
    {
        // Act
        var result = await _skillService.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Name.Should().Be(".NET Core");
        result.Category.Should().Be("Programming Languages");
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingSkill_ReturnsNull()
    {
        // Act
        var result = await _skillService.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_CachedSkill_ReturnsCachedResult()
    {
        // Arrange
        var cachedDto = new SkillDto
        {
            Id = 1,
            Name = "Cached Skill",
            Category = "Cached Category",
            IsActive = true,
            CreatedDate = DateTime.UtcNow,
            ModifiedDate = DateTime.UtcNow
        };

        _cache.Set("skill_1", cachedDto);

        // Act
        var result = await _skillService.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Cached Skill");
        result.Category.Should().Be("Cached Category");
    }

    [Fact]
    public async Task GetAllAsync_ReturnsOnlyActiveSkills()
    {
        // Arrange

        // Act
        var result = await _skillService.GetAllAsync(true);

        // Assert
        var skills = result.ToList();
        skills.Should().HaveCount(2);
        skills.Should().OnlyContain(s => s.IsActive);
        skills.Should().Contain(s => s.Name == ".NET Core");
        skills.Should().Contain(s => s.Name == "React");
        skills.Should().NotContain(s => s.Name == "Obsolete Technology");
    }

    [Fact]
    public async Task GetAllAsync_IncludeInactive_ReturnsAllSkills()
    {
        // Arrange

        // Act
        var result = await _skillService.GetAllAsync(false);

        // Assert
        var skills = result.ToList();
        skills.Should().HaveCount(3);
        skills.Should().Contain(s => s.Name == ".NET Core");
        skills.Should().Contain(s => s.Name == "React");
        skills.Should().Contain(s => s.Name == "Obsolete Technology");
    }

    [Fact]
    public async Task GetByCategoryAsync_ReturnsSkillsInCategory()
    {
        // Arrange

        // Act
        var result = await _skillService.GetByCategoryAsync("Programming Languages", true);

        // Assert
        var skills = result.ToList();
        skills.Should().HaveCount(1);
        skills.First().Name.Should().Be(".NET Core");
        skills.First().Category.Should().Be("Programming Languages");
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsCreatedSkill()
    {
        // Arrange
        var request = new CreateSkillRequest
        {
            Name = "Vue.js",
            Category = "Frontend Frameworks",
            IsActive = true
        };

        // Act
        var result = await _skillService.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Vue.js");
        result.Category.Should().Be("Frontend Frameworks");
        result.IsActive.Should().BeTrue();
        result.CreatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

        // Verify in database
        var skillInDb = await _context.Skills.FirstAsync(s => s.Id == result.Id);
        skillInDb.Name.Should().Be("Vue.js");
        skillInDb.Category.Should().Be("Frontend Frameworks");
        skillInDb.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_ExistingSkill_ReturnsUpdatedSkill()
    {
        // Arrange
        var request = new UpdateSkillRequest
        {
            Name = "Updated .NET Core",
            Category = "Updated Programming Languages",
            IsActive = true
        };

        // Act
        var result = await _skillService.UpdateAsync(1, request);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated .NET Core");
        result.Category.Should().Be("Updated Programming Languages");
        result.ModifiedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

        // Verify cache was cleared by checking it's not there anymore
        _cache.TryGetValue("skill_1", out _).Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_NonExistingSkill_ReturnsNull()
    {
        // Arrange
        var request = new UpdateSkillRequest
        {
            Name = "Non-existing Skill",
            Category = "Category",
            IsActive = true
        };

        // Act
        var result = await _skillService.UpdateAsync(999, request);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_ExistingSkillWithoutJobPositions_ReturnsTrue()
    {
        // Act
        var result = await _skillService.DeleteAsync(3); // Obsolete Technology skill

        // Assert
        result.Should().BeTrue();

        // Verify skill is deleted from database
        var skillInDb = await _context.Skills.FirstOrDefaultAsync(s => s.Id == 3);
        skillInDb.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_ExistingSkillWithJobPositions_SoftDeletes()
    {
        // Arrange
        var jobPositionSkill = new JobPositionSkill
        {
            JobPositionId = 1,
            SkillId = 1,
            IsRequired = true,
            RequiredLevel = "Intermediate"
        };
        _context.JobPositionSkills.Add(jobPositionSkill);
        await _context.SaveChangesAsync();

        // Act
        var result = await _skillService.DeleteAsync(1);

        // Assert
        result.Should().BeTrue();

        // Verify skill is marked as inactive
        var skillInDb = await _context.Skills.FirstOrDefaultAsync(s => s.Id == 1);
        skillInDb.Should().NotBeNull();
        skillInDb!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_NonExistingSkill_ReturnsFalse()
    {
        // Act
        var result = await _skillService.DeleteAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsAsync_ExistingSkill_ReturnsTrue()
    {
        // Act
        var result = await _skillService.ExistsAsync(1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_NonExistingSkill_ReturnsFalse()
    {
        // Act
        var result = await _skillService.ExistsAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsByNameAsync_ExistingSkill_ReturnsTrue()
    {
        // Act
        var result = await _skillService.ExistsByNameAsync(".NET Core");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByNameAsync_NonExistingSkill_ReturnsFalse()
    {
        // Act
        var result = await _skillService.ExistsByNameAsync("Non-existing Skill");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsByNameAsync_WithExclusion_ReturnsCorrectResult()
    {
        // Act - exclude the existing skill with ID 1
        var result = await _skillService.ExistsByNameAsync(".NET Core", 1);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetCategoriesAsync_ReturnsUniqueCategories()
    {
        // Arrange
        // No cache setup needed

        // Act
        var result = await _skillService.GetCategoriesAsync();

        // Assert
        var categories = result.ToList();
        categories.Should().HaveCount(2); // Only active skills count
        categories.Should().Contain("Programming Languages");
        categories.Should().Contain("Frontend Frameworks");
        categories.Should().NotContain("Legacy"); // Inactive skill category
        categories.Should().BeInAscendingOrder();
    }

    [Fact]
    public async Task GetCategoriesAsync_CachedCategories_ReturnsCachedResult()
    {
        // Arrange
        var cachedCategories = new[] { "Cached Category 1", "Cached Category 2" };
        _cache.Set("skill_categories", cachedCategories);

        // Act
        var result = await _skillService.GetCategoriesAsync();

        // Assert
        var categories = result.ToList();
        categories.Should().HaveCount(2);
        categories.Should().Contain("Cached Category 1");
        categories.Should().Contain("Cached Category 2");
    }

    [Fact]
    public async Task SearchAsync_WithSearchTerm_ReturnsMatchingSkills()
    {
        // Act
        var result = await _skillService.SearchAsync(".NET");

        // Assert
        var skills = result.ToList();
        skills.Should().HaveCount(1);
        skills.First().Name.Should().Be(".NET Core");
    }

    [Fact]
    public async Task SearchAsync_WithEmptySearchTerm_ReturnsAllActiveSkills()
    {
        // Act
        var result = await _skillService.SearchAsync("");

        // Assert
        var skills = result.ToList();
        skills.Should().HaveCount(2); // Only active skills
        skills.Should().OnlyContain(s => s.IsActive);
    }

    [Fact]
    public async Task SearchAsync_WithCategorySearch_ReturnsMatchingSkills()
    {
        // Act
        var result = await _skillService.SearchAsync("Programming");

        // Assert
        var skills = result.ToList();
        skills.Should().HaveCount(1);
        skills.First().Category.Should().Be("Programming Languages");
    }

    [Fact]
    public async Task GetByCategoryAsync_CachesResults()
    {
        // Arrange

        // Act
        await _skillService.GetByCategoryAsync("Programming Languages", true);

        // Assert
        // Verify cache was set
        _cache.TryGetValue("skills_category_Programming Languages_True", out _).Should().BeTrue();
    }

    [Fact]
    public async Task GetAllAsync_CachesResults()
    {
        // Arrange

        // Act
        await _skillService.GetAllAsync(true);

        // Assert
        // Verify cache was set
        _cache.TryGetValue("skills_all_True", out _).Should().BeTrue();
    }

    public void Dispose()
    {
        _context.Dispose();
        _cache.Dispose();
    }
}