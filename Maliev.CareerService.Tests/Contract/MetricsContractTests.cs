using FluentAssertions;
using System.Net;
using System.Net.Http;
using Xunit;
using Maliev.CareerService.Tests.Helpers;

namespace Maliev.CareerService.Tests.Contract;

/// <summary>
/// Contract tests for Prometheus metrics endpoint
/// </summary>
public class MetricsContractTests(CareerServiceFactory factory) : BaseIntegrationTest(factory)
{
    [DockerRequiredFact]
    public async Task MetricsEndpoint_ReturnsTextPlainContentType()
    {
        // Act
        var response = await Client.GetAsync("/careers/metrics");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Prometheus metrics MUST be text/plain
        response.Content.Headers.ContentType.Should().NotBeNull();
        response.Content.Headers.ContentType!.MediaType.Should().Be("text/plain");

        // Charset should be UTF-8
        response.Content.Headers.ContentType.CharSet.Should().BeOneOf("utf-8", "UTF-8", null);
    }

    [DockerRequiredFact]
    public async Task MetricsEndpoint_ReturnsNonEmptyContent()
    {
        // Act
        var response = await Client.GetAsync("/careers/metrics");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Length.Should().BeGreaterThan(100); // Reasonable minimum for metrics output
    }

    [DockerRequiredFact]
    public async Task MetricsEndpoint_FollowsPrometheusLineFormat()
    {
        // Act
        var response = await Client.GetAsync("/careers/metrics");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var lines = content.Split('\n');

        // Each line should be:
        // - Empty
        // - A comment (starts with #)
        // - A metric (metric_name{labels} value timestamp?)
        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (string.IsNullOrWhiteSpace(trimmed))
                continue; // Empty lines are OK

            if (trimmed.StartsWith('#'))
                continue; // Comment lines are OK

            // Metric line: should contain at least metric_name and value
            // Format: metric_name{label="value"} 123.45 1234567890
            // Labels can contain quoted strings with any characters (including braces)
            trimmed.Should().MatchRegex(@"^[a-zA-Z_:][a-zA-Z0-9_:]*(\{.+?\})?\s+[\d\.\+\-eE]+(\s+\d+)?$");
        }
    }

    [DockerRequiredFact]
    public async Task MetricsEndpoint_ContainsHelpComments()
    {
        // Act
        var response = await Client.GetAsync("/careers/metrics");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();

        // Prometheus format requires HELP comments
        content.Should().Contain("# HELP");

        var lines = content.Split('\n');
        var helpLines = lines.Where(l => l.StartsWith("# HELP")).ToList();
        helpLines.Should().HaveCountGreaterThan(0);

        // Each HELP line should follow format: # HELP metric_name description
        foreach (var helpLine in helpLines)
        {
            helpLine.Should().MatchRegex(@"^# HELP\s+[a-zA-Z_:][a-zA-Z0-9_:]*\s+.+$");
        }
    }

    [DockerRequiredFact]
    public async Task MetricsEndpoint_ContainsTypeComments()
    {
        // Act
        var response = await Client.GetAsync("/careers/metrics");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();

        // Prometheus format requires TYPE comments
        content.Should().Contain("# TYPE");

        var lines = content.Split('\n');
        var typeLines = lines.Where(l => l.StartsWith("# TYPE")).ToList();
        typeLines.Should().HaveCountGreaterThan(0);

        // Each TYPE line should follow format: # TYPE metric_name (counter|gauge|histogram|summary)
        foreach (var typeLine in typeLines)
        {
            typeLine.Should().MatchRegex(@"^# TYPE\s+[a-zA-Z_:][a-zA-Z0-9_:]*\s+(counter|gauge|histogram|summary|untyped)$");
        }
    }

    [DockerRequiredFact]
    public async Task MetricsEndpoint_SupportsHttpGet()
    {
        // Act
        var response = await Client.GetAsync("/careers/metrics");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [DockerRequiredFact]
    public async Task MetricsEndpoint_RejectsHttpPost()
    {
        // Act
        var response = await Client.PostAsync("/careers/metrics", new StringContent(string.Empty));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
    }

    [DockerRequiredFact]
    public async Task MetricsEndpoint_IsIdempotent()
    {
        // Act - Call twice
        var response1 = await Client.GetAsync("/careers/metrics");
        var content1 = await response1.Content.ReadAsStringAsync();

        await Task.Delay(100); // Small delay

        var response2 = await Client.GetAsync("/careers/metrics");
        var content2 = await response2.Content.ReadAsStringAsync();

        // Assert - Both should succeed
        response1.StatusCode.Should().Be(HttpStatusCode.OK);
        response2.StatusCode.Should().Be(HttpStatusCode.OK);

        // Content should have same structure (though values may differ)
        content1.Should().Contain("# HELP");
        content2.Should().Contain("# HELP");

        var lines1 = content1.Split('\n').Where(l => l.StartsWith("# HELP")).ToList();
        var lines2 = content2.Split('\n').Where(l => l.StartsWith("# HELP")).ToList();

        // Same number of metric definitions
        lines1.Count.Should().Be(lines2.Count);
    }
}
