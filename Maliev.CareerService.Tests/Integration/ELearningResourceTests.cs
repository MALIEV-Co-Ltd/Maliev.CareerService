using Maliev.CareerService.Api.Models.ELearningResources;
using Maliev.CareerService.Api.Authentication;
using Maliev.CareerService.Data.Models;
using Maliev.CareerService.Tests.Factories;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Maliev.CareerService.Tests.Integration;

/// <summary>
/// Integration tests for e-learning resources functionality
/// </summary>
public class ELearningResourceTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ELearningResourceTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();

        // Add Employee authorization header with Read permission
        _client.DefaultRequestHeaders.Add("Authorization", "Bearer " + _factory.CreateTestJwtToken("employee-id", null, new[] { CareerPermissions.Trainings.Read }));
    }

    [Fact]
    public async Task GetELearningResources_WithoutFilters_ReturnsActiveResources()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var response = await _client.GetAsync("/career/v1/elearning-resources");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ELearningResourceListResponse>();
        Assert.NotNull(result);
        Assert.NotEmpty(result!.Items);
        Assert.True(result.TotalCount > 0);
        Assert.All(result.Items, r => Assert.True(r.IsActive));
    }

    [Fact]
    public async Task GetELearningResources_WithCategoryFilter_ReturnsFilteredResources()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var response = await _client.GetAsync("/career/v1/elearning-resources?category=Technical");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ELearningResourceListResponse>();
        Assert.NotNull(result);
        Assert.All(result!.Items, r => Assert.Equal("Technical", r.Category));
    }

    [Fact]
    public async Task GetELearningResources_WithResourceTypeFilter_ReturnsFilteredResources()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var response = await _client.GetAsync("/career/v1/elearning-resources?resourceType=Video");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ELearningResourceListResponse>();
        Assert.NotNull(result);
        Assert.All(result!.Items, r => Assert.Equal("Video", r.ResourceType));
    }

    [Fact]
    public async Task GetELearningResources_WithCategoryAndTypeFilter_ReturnsFilteredResources()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var response = await _client.GetAsync("/career/v1/elearning-resources?category=Leadership&resourceType=Document");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ELearningResourceListResponse>();
        Assert.NotNull(result);
        Assert.All(result!.Items, r => { Assert.Equal("Leadership", r.Category); Assert.Equal("Document", r.ResourceType); });
    }

    [Fact]
    public async Task GetELearningResources_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var response = await _client.GetAsync("/career/v1/elearning-resources?offset=0&limit=2");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ELearningResourceListResponse>();
        Assert.NotNull(result);
        Assert.True(result!.Items.Count <= 2);
        Assert.Equal(1, result.Page);
        Assert.Equal(2, result.PageSize);
    }

    [Fact]
    public async Task GetELearningResources_WithInvalidLimit_ReturnsBadRequest()
    {
        // Arrange - limit too high
        // Act
        var response = await _client.GetAsync("/career/v1/elearning-resources?limit=101");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Limit must be between 1 and 100", content);
    }

    [Fact]
    public async Task GetELearningResourceById_ExistingId_ReturnsResource()
    {
        // Arrange
        var resourceId = await SeedSingleResourceAsync();

        // Act
        var response = await _client.GetAsync($"/career/v1/elearning-resources/{resourceId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ELearningResourceResponse>();
        Assert.NotNull(result);
        Assert.Equal(resourceId, result!.Id);
        Assert.False(string.IsNullOrEmpty(result.Title));
        Assert.False(string.IsNullOrEmpty(result.ResourceType));
    }

    [Fact]
    public async Task GetELearningResourceById_WithExternalLmsUrl_ReturnsResourceWithUrl()
    {
        // Arrange
        var resourceId = await SeedResourceWithExternalLmsAsync();

        // Act
        var response = await _client.GetAsync($"/career/v1/elearning-resources/{resourceId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ELearningResourceResponse>();
        Assert.NotNull(result);
        Assert.Equal(resourceId, result!.Id);
        Assert.False(string.IsNullOrEmpty(result.ExternalLmsUrl));
        Assert.StartsWith("https://", result.ExternalLmsUrl);
    }

    [Fact]
    public async Task GetELearningResourceById_NonExistingId_ReturnsNotFound()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/career/v1/elearning-resources/{nonExistingId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetELearningResources_WithAdminPermission_ReturnsResources()
    {
        // Arrange
        await SeedTestDataAsync();

        _client.DefaultRequestHeaders.Remove("Authorization");
        _client.DefaultRequestHeaders.Add("Authorization", "Bearer " + _factory.CreateTestJwtToken("hr-staff-id", null, new[] { "career.*" }));

        // Act
        var response = await _client.GetAsync("/career/v1/elearning-resources");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ELearningResourceListResponse>();
        Assert.NotNull(result);
        Assert.NotEmpty(result!.Items);
    }

    [Fact]
    public async Task GetELearningResources_Unauthenticated_ReturnsUnauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Remove("Authorization");

        // Act
        var response = await _client.GetAsync("/career/v1/elearning-resources");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private async Task SeedTestDataAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<Data.CareerDbContext>();

        // Clear existing data
        dbContext.ELearningResources.RemoveRange(dbContext.ELearningResources);
        await dbContext.SaveChangesAsync();

        // Add test data
        var resources = new List<ELearningResource>
        {
            new() {
                Id = Guid.NewGuid(),
                ResourceCode = "TECH-VID-001",
                Title = "Introduction to Cloud Computing",
                Description = "Learn the fundamentals of cloud computing",
                Category = "Technical",
                ResourceType = ELearningResourceType.Video,
                EstimatedMinutes = 45,
                IsActive = true,
                CreatedBy = Guid.NewGuid(),
                UpdatedBy = Guid.NewGuid()
            },
            new() {
                Id = Guid.NewGuid(),
                ResourceCode = "LEAD-DOC-001",
                Title = "Effective Communication for Leaders",
                Description = "Best practices for leadership communication",
                Category = "Leadership",
                ResourceType = ELearningResourceType.Document,
                EstimatedMinutes = 30,
                IsActive = true,
                CreatedBy = Guid.NewGuid(),
                UpdatedBy = Guid.NewGuid()
            },
            new() {
                Id = Guid.NewGuid(),
                ResourceCode = "TECH-INT-001",
                Title = "Interactive Python Programming",
                Description = "Hands-on Python programming exercises",
                Category = "Technical",
                ResourceType = ELearningResourceType.Interactive,
                ExternalLmsUrl = "https://codecademy.com/learn/python",
                EstimatedMinutes = 120,
                IsActive = true,
                CreatedBy = Guid.NewGuid(),
                UpdatedBy = Guid.NewGuid()
            },
            new() {
                Id = Guid.NewGuid(),
                ResourceCode = "COMP-QUIZ-001",
                Title = "Data Privacy Compliance Quiz",
                Description = "Test your knowledge of GDPR and data protection",
                Category = "Compliance",
                ResourceType = ELearningResourceType.Quiz,
                EstimatedMinutes = 15,
                IsActive = true,
                CreatedBy = Guid.NewGuid(),
                UpdatedBy = Guid.NewGuid()
            },
            new() {
                Id = Guid.NewGuid(),
                ResourceCode = "TECH-VID-002",
                Title = "Advanced Kubernetes Deployment",
                Description = "Master Kubernetes deployments and orchestration",
                Category = "Technical",
                ResourceType = ELearningResourceType.Video,
                ExternalLmsUrl = "https://linkedin.com/learning/kubernetes-advanced",
                EstimatedMinutes = 90,
                IsActive = true,
                CreatedBy = Guid.NewGuid(),
                UpdatedBy = Guid.NewGuid()
            }
        };

        dbContext.ELearningResources.AddRange(resources);
        await dbContext.SaveChangesAsync();
    }

    private async Task<Guid> SeedSingleResourceAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<Data.CareerDbContext>();

        var resource = new ELearningResource
        {
            Id = Guid.NewGuid(),
            ResourceCode = "TEST-VID-999",
            Title = "Test E-Learning Resource",
            Description = "Test resource for integration tests",
            Category = "Testing",
            ResourceType = ELearningResourceType.Video,
            EstimatedMinutes = 30,
            IsActive = true,
            CreatedBy = Guid.NewGuid(),
            UpdatedBy = Guid.NewGuid()
        };

        dbContext.ELearningResources.Add(resource);
        await dbContext.SaveChangesAsync();

        return resource.Id;
    }

    private async Task<Guid> SeedResourceWithExternalLmsAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<Data.CareerDbContext>();

        var resource = new ELearningResource
        {
            Id = Guid.NewGuid(),
            ResourceCode = "EXT-VID-999",
            Title = "External LMS Resource",
            Description = "Resource with external LMS URL",
            Category = "Technical",
            ResourceType = ELearningResourceType.Video,
            ExternalLmsUrl = "https://udemy.com/course/test-course",
            EstimatedMinutes = 60,
            IsActive = true,
            CreatedBy = Guid.NewGuid(),
            UpdatedBy = Guid.NewGuid()
        };

        dbContext.ELearningResources.Add(resource);
        await dbContext.SaveChangesAsync();

        return resource.Id;
    }
}
