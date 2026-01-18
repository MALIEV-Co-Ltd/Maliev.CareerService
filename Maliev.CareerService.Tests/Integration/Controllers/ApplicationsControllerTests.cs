using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Maliev.CareerService.Api.Models.Applications;
using Maliev.CareerService.Data.Models;
using Xunit;

namespace Maliev.CareerService.Tests.Integration.Controllers;

public class ApplicationsControllerTests : BaseIntegrationTest
{
    public ApplicationsControllerTests(CareerServiceFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetApplicantApplications_ShouldReturnOk()
    {
        // Arrange
        var email = "john.doe@maliev.com";
        var userId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var additionalClaims = new Dictionary<string, string> { { "email", email } };
        var token = Factory.CreateTestJwtToken(userId.ToString(), new[] { "Employee" }, new[] { "Permission:career.applications.read" }, additionalClaims);

        var posting = new JobPosting
        {
            Id = Guid.NewGuid(),
            PositionTitle = "Engineer",
            PositionCode = "ENG002",
            Description = "Desc",
            Requirements = "Req",
            Responsibilities = "Resp",
            EmploymentType = "Full-time",
            IsActive = true,
            PublishedAt = DateTime.UtcNow.AddDays(-1),
            ApplicationDeadline = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var application = new JobApplication
        {
            Id = Guid.NewGuid(),
            JobPostingId = posting.Id,
            ApplicantEmail = email,
            ApplicantFirstName = "John",
            ApplicantLastName = "Doe",
            Status = "Submitted",
            AppliedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await SeedDatabaseAsync(posting, application);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/career/v1/job-applications?email={email}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await Client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<JobApplicationListResponse>();
        Assert.NotNull(result);
        Assert.NotEmpty(result.Items);
    }
}
