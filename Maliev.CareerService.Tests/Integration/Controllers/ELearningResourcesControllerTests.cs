using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Maliev.CareerService.Api.Models.ELearningResources;
using Maliev.CareerService.Data.Models;
using Xunit;

namespace Maliev.CareerService.Tests.Integration.Controllers;

public class ELearningResourcesControllerTests : BaseIntegrationTest
{
    public ELearningResourcesControllerTests(CareerServiceFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetELearningResources_ShouldReturnOk()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var token = GenerateEmployeeToken(userId);

        var resource = new ELearningResource
        {
            Id = Guid.NewGuid(),
            ResourceCode = "VID-CS-001",
            Title = "Introduction to C#",
            Description = "Learn the basics of C#",
            Category = "Programming",
            ResourceType = "Video",
            ExternalLmsUrl = "https://example.com/csharp",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await SeedDatabaseAsync(resource);

        var request = new HttpRequestMessage(HttpMethod.Get, "/career/v1/elearning-resources");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await Client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ELearningResourceListResponse>();
        Assert.NotNull(result);
        Assert.NotEmpty(result.Items);
    }
}
