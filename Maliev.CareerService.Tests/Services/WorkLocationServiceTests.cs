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

public class WorkLocationServiceTests : IDisposable
{
    private readonly CareerDbContext _context;
    private readonly Mock<ILogger<WorkLocationService>> _mockLogger;
    private readonly IMemoryCache _cache;
    private readonly CacheOptions _cacheOptions;
    private readonly WorkLocationService _workLocationService;

    public WorkLocationServiceTests()
    {
        var options = new DbContextOptionsBuilder<CareerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new CareerDbContext(options);
        _mockLogger = new Mock<ILogger<WorkLocationService>>();
        _cache = new MemoryCache(new MemoryCacheOptions());
        _cacheOptions = new CacheOptions 
        { 
            MaxCacheSize = 100,
            DefaultExpiration = TimeSpan.FromMinutes(30),
            LongExpiration = TimeSpan.FromHours(2)
        };

        _workLocationService = new WorkLocationService(
            _context,
            _cache,
            _mockLogger.Object,
            _cacheOptions);

        SeedTestData();
    }

    private void SeedTestData()
    {
        var locations = new[]
        {
            new WorkLocation
            {
                Id = 1,
                Name = "Bangkok Office",
                Address = "123 Sukhumvit Road",
                City = "Bangkok",
                CountryId = 1,
                IsActive = true,
                IsRemoteAllowed = false,
                IsHybrid = true
            },
            new WorkLocation
            {
                Id = 2,
                Name = "Chiang Mai Office",
                Address = "456 Nimman Road",
                City = "Chiang Mai",
                CountryId = 1,
                IsActive = true,
                IsRemoteAllowed = true,
                IsHybrid = false
            },
            new WorkLocation
            {
                Id = 3,
                Name = "Closed Office",
                Address = "789 Old Road",
                City = "Bangkok",
                CountryId = 1,
                IsActive = false,
                IsRemoteAllowed = false,
                IsHybrid = false
            }
        };

        _context.WorkLocations.AddRange(locations);
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetByIdAsync_ExistingLocation_ReturnsWorkLocationDto()
    {
        // Arrange

        // Act
        var result = await _workLocationService.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Name.Should().Be("Bangkok Office");
        result.Address.Should().Be("123 Sukhumvit Road");
        result.City.Should().Be("Bangkok");
        result.CountryId.Should().Be(1);
        result.IsActive.Should().BeTrue();
        result.IsRemoteAllowed.Should().BeFalse();
        result.IsHybrid.Should().BeTrue();
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingLocation_ReturnsNull()
    {
        // Arrange

        // Act
        var result = await _workLocationService.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_CachedLocation_ReturnsCachedResult()
    {
        // Arrange
        var cachedDto = new WorkLocationDto
        {
            Id = 1,
            Name = "Cached Office",
            Address = "Cached Address",
            City = "Cached City",
            CountryId = 2,
            IsActive = true,
            IsRemoteAllowed = true,
            IsHybrid = true,
            CreatedDate = DateTime.UtcNow,
            ModifiedDate = DateTime.UtcNow
        };

        _cache.Set("worklocation_1", cachedDto);

        // Act
        var result = await _workLocationService.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Cached Office");
        result.Address.Should().Be("Cached Address");
    }

    [Fact]
    public async Task GetAllAsync_ReturnsOnlyActiveLocations()
    {
        // Arrange

        // Act
        var result = await _workLocationService.GetAllAsync(true);

        // Assert
        var locations = result.ToList();
        locations.Should().HaveCount(2);
        locations.Should().OnlyContain(l => l.IsActive);
        locations.Should().Contain(l => l.Name == "Bangkok Office");
        locations.Should().Contain(l => l.Name == "Chiang Mai Office");
        locations.Should().NotContain(l => l.Name == "Closed Office");
    }

    [Fact]
    public async Task GetAllAsync_IncludeInactive_ReturnsAllLocations()
    {
        // Arrange

        // Act
        var result = await _workLocationService.GetAllAsync(false);

        // Assert
        var locations = result.ToList();
        locations.Should().HaveCount(3);
        locations.Should().Contain(l => l.Name == "Bangkok Office");
        locations.Should().Contain(l => l.Name == "Chiang Mai Office");
        locations.Should().Contain(l => l.Name == "Closed Office");
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsCreatedLocation()
    {
        // Arrange
        var request = new CreateWorkLocationRequest
        {
            Name = "Phuket Office",
            Address = "789 Beach Road",
            City = "Phuket",
            CountryId = 1,
            IsRemoteAllowed = true,
            IsHybrid = true,
            IsActive = true
        };

        // Act
        var result = await _workLocationService.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Phuket Office");
        result.Address.Should().Be("789 Beach Road");
        result.City.Should().Be("Phuket");
        result.CountryId.Should().Be(1);
        result.IsRemoteAllowed.Should().BeTrue();
        result.IsHybrid.Should().BeTrue();
        result.IsActive.Should().BeTrue();
        result.CreatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

        // Verify in database
        var locationInDb = await _context.WorkLocations.FirstAsync(l => l.Id == result.Id);
        locationInDb.Name.Should().Be("Phuket Office");
        locationInDb.Address.Should().Be("789 Beach Road");
        locationInDb.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_ExistingLocation_ReturnsUpdatedLocation()
    {
        // Arrange
        var request = new UpdateWorkLocationRequest
        {
            Name = "Updated Bangkok Office",
            Address = "Updated Address",
            City = "Updated City",
            CountryId = 2,
            IsRemoteAllowed = true,
            IsHybrid = false,
            IsActive = true
        };

        // Act
        var result = await _workLocationService.UpdateAsync(1, request);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated Bangkok Office");
        result.Address.Should().Be("Updated Address");
        result.City.Should().Be("Updated City");
        result.CountryId.Should().Be(2);
        result.IsRemoteAllowed.Should().BeTrue();
        result.IsHybrid.Should().BeFalse();
        result.ModifiedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

        // Verify cache was cleared
        _cache.TryGetValue("worklocation_1", out _).Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_NonExistingLocation_ReturnsNull()
    {
        // Arrange
        var request = new UpdateWorkLocationRequest
        {
            Name = "Non-existing Location",
            Address = "Address",
            City = "City",
            CountryId = 1,
            IsRemoteAllowed = false,
            IsHybrid = false,
            IsActive = true
        };

        // Act
        var result = await _workLocationService.UpdateAsync(999, request);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_ExistingLocationWithoutJobPositions_ReturnsTrue()
    {
        // Act
        var result = await _workLocationService.DeleteAsync(2);

        // Assert
        result.Should().BeTrue();

        // Verify location is deleted from database
        var locationInDb = await _context.WorkLocations.FirstOrDefaultAsync(l => l.Id == 2);
        locationInDb.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_ExistingLocationWithJobPositions_SoftDeletes()
    {
        // Arrange
        var jobPositionLocation = new JobPositionLocation
        {
            JobPositionId = 1,
            WorkLocationId = 1
        };
        _context.JobPositionLocations.Add(jobPositionLocation);
        await _context.SaveChangesAsync();

        // Act
        var result = await _workLocationService.DeleteAsync(1);

        // Assert
        result.Should().BeTrue();

        // Verify location is marked as inactive
        var locationInDb = await _context.WorkLocations.FirstOrDefaultAsync(l => l.Id == 1);
        locationInDb.Should().NotBeNull();
        locationInDb!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_NonExistingLocation_ReturnsFalse()
    {
        // Act
        var result = await _workLocationService.DeleteAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsAsync_ExistingLocation_ReturnsTrue()
    {
        // Act
        var result = await _workLocationService.ExistsAsync(1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_NonExistingLocation_ReturnsFalse()
    {
        // Act
        var result = await _workLocationService.ExistsAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsByNameAndCityAsync_ExistingLocation_ReturnsTrue()
    {
        // Act
        var result = await _workLocationService.ExistsByNameAndCityAsync("Bangkok Office", "Bangkok");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByNameAndCityAsync_NonExistingLocation_ReturnsFalse()
    {
        // Act
        var result = await _workLocationService.ExistsByNameAndCityAsync("Non-existing Office", "Non-existing City");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsByNameAndCityAsync_WithExclusion_ReturnsCorrectResult()
    {
        // Act - exclude the existing location with ID 1
        var result = await _workLocationService.ExistsByNameAndCityAsync("Bangkok Office", "Bangkok", 1);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetCitiesAsync_ReturnsUniqueCities()
    {
        // Arrange

        // Act
        var result = await _workLocationService.GetCitiesAsync();

        // Assert
        var cities = result.ToList();
        cities.Should().HaveCount(2);
        cities.Should().Contain("Bangkok");
        cities.Should().Contain("Chiang Mai");
        cities.Should().BeInAscendingOrder();
    }

    [Fact]
    public async Task GetCitiesAsync_CachedCities_ReturnsCachedResult()
    {
        // Arrange
        var cachedCities = new[] { "Cached City 1", "Cached City 2" };
        _cache.Set("worklocation_cities", cachedCities);

        // Act
        var result = await _workLocationService.GetCitiesAsync();

        // Assert
        var cities = result.ToList();
        cities.Should().HaveCount(2);
        cities.Should().Contain("Cached City 1");
        cities.Should().Contain("Cached City 2");
    }

    [Fact]
    public async Task GetAllAsync_CachesResults()
    {
        // Arrange

        // Act
        await _workLocationService.GetAllAsync(true);

        // Assert
        // Verify cache was set
        _cache.TryGetValue("worklocations_all_True", out _).Should().BeTrue();
    }

    public void Dispose()
    {
        _context.Dispose();
        _cache.Dispose();
    }
}