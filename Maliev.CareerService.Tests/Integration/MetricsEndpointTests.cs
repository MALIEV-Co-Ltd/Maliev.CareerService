using FluentAssertions;
using System.Net;
using System.Net.Http;
using Xunit;
using Maliev.CareerService.Tests.Helpers;

namespace Maliev.CareerService.Tests.Integration;

/// <summary>
/// Integration tests for Prometheus metrics endpoint
/// </summary>
public class MetricsEndpointTests(CareerServiceFactory factory) : BaseIntegrationTest(factory)
{
    [DockerRequiredFact]
    public async Task GetMetrics_ReturnsPrometheusFormat()
    {
        // Act
        var response = await Client.GetAsync("/careers/metrics");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("text/plain");

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();

        // Verify Prometheus format (contains HELP and TYPE comments)
        content.Should().Contain("# HELP");
        content.Should().Contain("# TYPE");
    }

    [DockerRequiredFact]
    public async Task GetMetrics_ContainsHttpMetrics()
    {
        // Act
        var response = await Client.GetAsync("/careers/metrics");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();

        // Verify HTTP metrics are present
        content.Should().Contain("http_requests_received_total");
        content.Should().Contain("http_requests_in_progress");
    }

    [DockerRequiredFact]
    public async Task GetMetrics_ContainsJobApplicationMetrics()
    {
        // Act
        var response = await Client.GetAsync("/careers/metrics");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();

        // Verify custom job application metrics
        content.Should().Contain("career_job_applications_total");
    }

    [DockerRequiredFact]
    public async Task GetMetrics_ContainsTrainingEnrollmentMetrics()
    {
        // Act
        var response = await Client.GetAsync("/careers/metrics");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();

        // Verify custom training enrollment metrics
        content.Should().Contain("career_training_enrollments_total");
    }

    [DockerRequiredFact]
    public async Task GetMetrics_ContainsActiveJobPostingsGauge()
    {
        // Act
        var response = await Client.GetAsync("/careers/metrics");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();

        // Verify active job postings gauge
        content.Should().Contain("career_active_job_postings");
    }

    [DockerRequiredFact]
    public async Task GetMetrics_ContainsConcurrentUsersGauge()
    {
        // Act
        var response = await Client.GetAsync("/careers/metrics");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();

        // Verify concurrent users gauge
        content.Should().Contain("career_concurrent_users");
    }

    [DockerRequiredFact]
    public async Task GetMetrics_DoesNotExposePII()
    {
        // Act
        var response = await Client.GetAsync("/careers/metrics");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();

        // Verify no PII in metrics (email, names, personal identifiers)
        content.Should().NotContainAny(new[] { "@", "email", "firstname", "lastname", "ssn", "passport" });

        // Metric labels should only contain enums/codes, not personal data
        content.Should().NotMatchRegex(@"[A-Za-z]+\s+[A-Za-z]+"); // No "First Last" patterns
    }

    [DockerRequiredFact]
    public async Task GetMetrics_AllowsAnonymousAccess()
    {
        // Arrange - No authentication header set

        // Act
        var response = await Client.GetAsync("/careers/metrics");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
    }

    [DockerRequiredFact]
    public async Task GetMetrics_HasAcceptableResponseTime()
    {
        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var response = await Client.GetAsync("/careers/metrics");
        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Metrics endpoint should respond quickly (< 200ms)
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(200);
    }
}
