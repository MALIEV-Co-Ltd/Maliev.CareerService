using FluentAssertions;
using Maliev.CareerService.Api.Models.ELearningResources;
using Maliev.CareerService.Data.Models;
using Maliev.CareerService.Tests.Factories;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Xunit;
using Maliev.CareerService.Tests.Helpers;

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

        // Add Employee authorization header
        _client.DefaultRequestHeaders.Add("Authorization", "Bearer Employee employee@example.com");
    }

    [DockerRequiredFact]
    public async Task GetELearningResources_WithoutFilters_ReturnsActiveResources()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var response = await _client.GetAsync("/careers/v1/elearning-resources");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ELearningResourceListResponse>();
        result.Should().NotBeNull();
        result!.Items.Should().NotBeEmpty();
        result.TotalCount.Should().BeGreaterThan(0);
        result.Items.Should().OnlyContain(r => r.IsActive);
    }

    [DockerRequiredFact]
    public async Task GetELearningResources_WithCategoryFilter_ReturnsFilteredResources()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var response = await _client.GetAsync("/careers/v1/elearning-resources?category=Technical");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ELearningResourceListResponse>();
        result.Should().NotBeNull();
        result!.Items.Should().OnlyContain(r => r.Category == "Technical");
    }

    [DockerRequiredFact]
    public async Task GetELearningResources_WithResourceTypeFilter_ReturnsFilteredResources()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var response = await _client.GetAsync("/careers/v1/elearning-resources?resourceType=Video");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ELearningResourceListResponse>();
        result.Should().NotBeNull();
        result!.Items.Should().OnlyContain(r => r.ResourceType == "Video");
    }

    [DockerRequiredFact]
    public async Task GetELearningResources_WithCategoryAndTypeFilter_ReturnsFilteredResources()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var response = await _client.GetAsync("/careers/v1/elearning-resources?category=Leadership&resourceType=Document");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ELearningResourceListResponse>();
        result.Should().NotBeNull();
        result!.Items.Should().OnlyContain(r => r.Category == "Leadership" && r.ResourceType == "Document");
    }

    [DockerRequiredFact]
    public async Task GetELearningResources_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var response = await _client.GetAsync("/careers/v1/elearning-resources?offset=0&limit=2");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ELearningResourceListResponse>();
        result.Should().NotBeNull();
        result!.Items.Should().HaveCountLessThanOrEqualTo(2);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(2);
    }

    [DockerRequiredFact]
    public async Task GetELearningResources_WithInvalidLimit_ReturnsBadRequest()
    {
        // Arrange - limit too high
        // Act
        var response = await _client.GetAsync("/careers/v1/elearning-resources?limit=101");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Limit must be between 1 and 100");
    }

    [DockerRequiredFact]
    public async Task GetELearningResourceById_ExistingId_ReturnsResource()
    {
        // Arrange
        var resourceId = await SeedSingleResourceAsync();

        // Act
        var response = await _client.GetAsync($"/careers/v1/elearning-resources/{resourceId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ELearningResourceResponse>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(resourceId);
        result.Title.Should().NotBeNullOrEmpty();
        result.ResourceType.Should().NotBeNullOrEmpty();
    }

    [DockerRequiredFact]
    public async Task GetELearningResourceById_WithExternalLmsUrl_ReturnsResourceWithUrl()
    {
        // Arrange
        var resourceId = await SeedResourceWithExternalLmsAsync();

        // Act
        var response = await _client.GetAsync($"/careers/v1/elearning-resources/{resourceId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ELearningResourceResponse>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(resourceId);
        result.ExternalLmsUrl.Should().NotBeNullOrEmpty();
        result.ExternalLmsUrl.Should().StartWith("https://");
    }

    [DockerRequiredFact]
    public async Task GetELearningResourceById_NonExistingId_ReturnsNotFound()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/careers/v1/elearning-resources/{nonExistingId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [DockerRequiredFact]
    public async Task GetELearningResources_AsHRStaff_ReturnsResources()
    {
        // Arrange
        await SeedTestDataAsync();

        _client.DefaultRequestHeaders.Remove("Authorization");
        _client.DefaultRequestHeaders.Add("Authorization", "Bearer HRStaff hr@example.com");

        // Act
        var response = await _client.GetAsync("/careers/v1/elearning-resources");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ELearningResourceListResponse>();
        result.Should().NotBeNull();
        result!.Items.Should().NotBeEmpty();
    }

    [DockerRequiredFact]
    public async Task GetELearningResources_Unauthenticated_ReturnsUnauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Remove("Authorization");

        // Act
        var response = await _client.GetAsync("/careers/v1/elearning-resources");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
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
