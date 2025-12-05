using System.Net;
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
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/plain", response.Content.Headers.ContentType?.MediaType);

        var content = await response.Content.ReadAsStringAsync();
        Assert.False(string.IsNullOrEmpty(content));

        // Verify Prometheus format (contains HELP and TYPE comments)
        Assert.Contains("# HELP", content);
        Assert.Contains("# TYPE", content);
    }

    [DockerRequiredFact]
    public async Task GetMetrics_ContainsHttpMetrics()
    {
        // Act
        var response = await Client.GetAsync("/careers/metrics");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();

        // Verify HTTP metrics are present (OpenTelemetry naming convention)
        // At least one of these http-related metrics should be present
        var hasHttpMetrics = content.Contains("http_server_") || 
                             content.Contains("http_client_") ||
                             content.Contains("http.server.") ||
                             content.Contains("aspnetcore_") ||
                             content.Contains("kestrel_");
        Assert.True(hasHttpMetrics, "Expected HTTP metrics to be present in Prometheus output");
    }

    [DockerRequiredFact]
    public async Task GetMetrics_ContainsDotNetMetrics()
    {
        // Act
        var response = await Client.GetAsync("/careers/metrics");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();

        // Verify .NET runtime metrics are present
        Assert.Contains("dotnet_", content);
    }

    [DockerRequiredFact]
    public async Task GetMetrics_ContainsProcessMetrics()
    {
        // Act
        var response = await Client.GetAsync("/careers/metrics");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();

        // Verify process metrics are present (memory, threads, etc.)
        var hasProcessMetrics = content.Contains("process_") || 
                                content.Contains("dotnet_gc_") ||
                                content.Contains("dotnet_jit_");
        Assert.True(hasProcessMetrics, "Expected process or runtime metrics to be present");
    }

    [DockerRequiredFact]
    public async Task GetMetrics_DoesNotExposePII()
    {
        // Act
        var response = await Client.GetAsync("/careers/metrics");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();

        // Verify no common PII patterns in metrics labels
        var lowerContent = content.ToLower();
        Assert.DoesNotContain("email=", lowerContent);
        Assert.DoesNotContain("firstname=", lowerContent);
        Assert.DoesNotContain("lastname=", lowerContent);
        Assert.DoesNotContain("ssn=", lowerContent);
        Assert.DoesNotContain("passport=", lowerContent);
        Assert.DoesNotContain("password=", lowerContent);
    }

    [DockerRequiredFact]
    public async Task GetMetrics_AllowsAnonymousAccess()
    {
        // Arrange - No authentication header set

        // Act
        var response = await Client.GetAsync("/careers/metrics");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [DockerRequiredFact]
    public async Task GetMetrics_HasAcceptableResponseTime()
    {
        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var response = await Client.GetAsync("/careers/metrics");
        stopwatch.Stop();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Metrics endpoint should respond quickly (<500ms for first request, may include warmup)
        Assert.True(stopwatch.ElapsedMilliseconds < 500, 
            $"Metrics endpoint took too long: {stopwatch.ElapsedMilliseconds}ms");
    }
}
