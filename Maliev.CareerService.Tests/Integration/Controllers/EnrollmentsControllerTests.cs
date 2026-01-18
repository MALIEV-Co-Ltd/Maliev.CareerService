using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Maliev.CareerService.Api.Models.Enrollments;
using Maliev.CareerService.Data.Models;
using Xunit;

namespace Maliev.CareerService.Tests.Integration.Controllers;

public class EnrollmentsControllerTests : BaseIntegrationTest
{
    public EnrollmentsControllerTests(CareerServiceFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetEnrollments_ShouldReturnOk()
    {
        // Arrange
        var userId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var token = GenerateEmployeeToken(userId);

        var request = new HttpRequestMessage(HttpMethod.Get, "/career/v1/training-enrollments");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await Client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<TrainingEnrollmentListResponse>();
        Assert.NotNull(result);
    }
}
