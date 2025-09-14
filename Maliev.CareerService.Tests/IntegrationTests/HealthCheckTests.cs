using System.Net;
using FluentAssertions;
using Maliev.CareerService.Tests.IntegrationTests;
using Xunit;

namespace Maliev.CareerService.Tests.IntegrationTests;

public class HealthCheckTests : IClassFixture<CareerServiceWebApplicationFactory>
{
    private readonly CareerServiceWebApplicationFactory _factory;

    public HealthCheckTests(CareerServiceWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task LivenessEndpoint_ReturnsOk()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/careers/liveness");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Be("Healthy");
    }
}