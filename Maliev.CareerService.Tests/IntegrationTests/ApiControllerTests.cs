using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Maliev.CareerService.Api.Models;
using Maliev.CareerService.Tests.IntegrationTests;
using Xunit;

namespace Maliev.CareerService.Tests.IntegrationTests;

public class ApiControllerTests : IClassFixture<CareerServiceWebApplicationFactory>
{
    private readonly CareerServiceWebApplicationFactory _factory;

    public ApiControllerTests(CareerServiceWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetDepartments_Returns_OK()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/careers/v1.0/positions/departments");

        // Assert
        // Even if there are no departments, it should return OK with an empty array
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var departments = await response.Content.ReadFromJsonAsync<IEnumerable<string>>();
        departments.Should().NotBeNull();
    }

    [Fact]
    public async Task GetSkills_Returns_OK()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/careers/v1.0/skills");

        // Assert
        // Even if there are no skills, it should return OK with an empty array
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var skills = await response.Content.ReadFromJsonAsync<IEnumerable<SkillDto>>();
        skills.Should().NotBeNull();
    }

    [Fact]
    public async Task GetSkillCategories_Returns_OK()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/careers/v1.0/skills/categories");

        // Assert
        // Even if there are no categories, it should return OK with an empty array
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var categories = await response.Content.ReadFromJsonAsync<IEnumerable<string>>();
        categories.Should().NotBeNull();
    }

    [Fact]
    public async Task SearchJobPositions_Returns_OK()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/careers/v1.0/positions/search");

        // Assert
        // Even if there are no positions, it should return OK with an empty paged result
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<JobPositionDto>>();
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetPublicJobPositions_Returns_OK()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/careers/v1.0/positions/public");

        // Assert
        // Even if there are no public positions, it should return OK with an empty paged result
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<JobPositionDto>>();
        result.Should().NotBeNull();
    }
}