using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Maliev.CareerService.Api.Models.DevelopmentPlans;
using Maliev.CareerService.Data.Models;
using Xunit;

namespace Maliev.CareerService.Tests.Integration.Controllers;

public class DevelopmentPlansControllerTests : BaseIntegrationTest
{
    public DevelopmentPlansControllerTests(CareerServiceFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetIDPs_ShouldReturnOk()
    {
        // Arrange
        var userId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var token = GenerateEmployeeToken(userId);

        var idp = new IndividualDevelopmentPlan
        {
            Id = Guid.NewGuid(),
            EmployeeId = userId,
            PlanYear = 2026,
            Status = "Draft",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await SeedDatabaseAsync(idp);

        var request = new HttpRequestMessage(HttpMethod.Get, "/career/v1/idps");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await Client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<IDPListResponse>();
        Assert.NotNull(result);
    }

    [Fact]
    public async Task CreateIDP_ShouldReturnCreated()
    {
        // Arrange
        var userId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var token = GenerateEmployeeToken(userId);

        var postData = new CreateIDPRequest
        {
            PlanYear = 2027
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "/career/v1/idps");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = JsonContent.Create(postData);

        // Act
        var response = await Client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<IDPResponse>();
        Assert.NotNull(result);
        Assert.Equal(2027, result.PlanYear);
    }
}
