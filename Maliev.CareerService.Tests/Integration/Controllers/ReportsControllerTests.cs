using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Maliev.CareerService.Application.Models.Reports;
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
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", hrToken);

        // Act
        var response = await Client.GetAsync("/career/v1/reports/training-compliance");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var report = await response.Content.ReadFromJsonAsync<TrainingComplianceReportDto>();
        Assert.NotNull(report);
    }

    [Fact]
    public async Task GetRecruitmentMetrics_ShouldReturnOk()
    {
        // Arrange
        var hrToken = GenerateHRStaffToken(Guid.NewGuid());
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", hrToken);

        // Act
        var response = await Client.GetAsync("/career/v1/reports/recruitment-metrics");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<RecruitmentMetricsResponse>();
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetLearningMetrics_ShouldReturnOk()
    {
        // Arrange
        var hrToken = GenerateHRStaffToken(Guid.NewGuid());
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", hrToken);

        // Act
        var response = await Client.GetAsync("/career/v1/reports/learning-metrics");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<LearningMetricsResponse>();
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetHROperationalMetrics_ShouldReturnOk()
    {
        // Arrange
        var hrToken = GenerateHRStaffToken(Guid.NewGuid());
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", hrToken);

        // Act
        var response = await Client.GetAsync("/career/v1/reports/hr-operational-metrics");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<HROperationalMetricsResponse>();
        Assert.NotNull(result);
    }
}
