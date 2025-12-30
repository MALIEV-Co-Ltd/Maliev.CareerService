using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Maliev.CareerService.Api.Models.Reports;
using Xunit;

namespace Maliev.CareerService.Tests.Integration.Controllers;

public class ReportsControllerTests : IntegrationTestBase
{
    public ReportsControllerTests(CareerServiceWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Get_TrainingComplianceReport_ShouldReturnOk()
    {
        // Arrange
        var hrToken = GenerateHRStaffToken(Guid.NewGuid());
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", hrToken);

        // Act
        var response = await Client.GetAsync("/career/v1/reports/training-compliance");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var report = await response.Content.ReadFromJsonAsync<TrainingComplianceReportDto>();
        report.Should().NotBeNull();
    }
}
