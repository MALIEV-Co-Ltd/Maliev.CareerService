using System.Net;
using System.Net.Http;
using Xunit;

namespace Maliev.CareerService.Tests.Contract;

/// <summary>
/// Contract tests for Prometheus metrics endpoint
/// </summary>
public class MetricsContractTests(CareerServiceFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task MetricsEndpoint_ReturnsTextPlainContentType()
    {
        // Act
        var response = await Client.GetAsync("/career/metrics");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Prometheus metrics MUST be text/plain
        Assert.NotNull(response.Content.Headers.ContentType);
        Assert.Equal("text/plain", response.Content.Headers.ContentType.MediaType);

        // Charset should be UTF-8
        Assert.True(response.Content.Headers.ContentType.CharSet == "utf-8" ||
                   response.Content.Headers.ContentType.CharSet == "UTF-8" ||
                   response.Content.Headers.ContentType.CharSet == null);
    }

    [Fact]
    public async Task MetricsEndpoint_ReturnsNonEmptyContent()
    {
        // Act
        var response = await Client.GetAsync("/career/metrics");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.False(string.IsNullOrEmpty(content));
        Assert.True(content.Length > 100); // Reasonable minimum for metrics output
    }

    [Fact]
    public async Task MetricsEndpoint_FollowsPrometheusLineFormat()
    {
        // Act
        var response = await Client.GetAsync("/career/metrics");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

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
            Assert.Matches(@"^[a-zA-Z_:][a-zA-Z0-9_:]*(\{.+?\})?\s+[\d\.\+\-eE]+(\s+\d+)?$", trimmed);
        }
    }

    [Fact]
    public async Task MetricsEndpoint_ContainsHelpComments()
    {
        // Act
        var response = await Client.GetAsync("/career/metrics");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();

        // Prometheus format requires HELP comments
        Assert.Contains("# HELP", content);

        var lines = content.Split('\n');
        var helpLines = lines.Where(l => l.StartsWith("# HELP")).ToList();
        Assert.True(helpLines.Count > 0);

        // Each HELP line should follow format: # HELP metric_name description
        foreach (var helpLine in helpLines)
        {
            Assert.Matches(@"^# HELP\s+[a-zA-Z_:][a-zA-Z0-9_:]*\s+.+$", helpLine);
        }
    }

    [Fact]
    public async Task MetricsEndpoint_ContainsTypeComments()
    {
        // Act
        var response = await Client.GetAsync("/career/metrics");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();

        // Prometheus format requires TYPE comments
        Assert.Contains("# TYPE", content);

        var lines = content.Split('\n');
        var typeLines = lines.Where(l => l.StartsWith("# TYPE")).ToList();
        Assert.True(typeLines.Count > 0);

        // Each TYPE line should follow format: # TYPE metric_name (counter|gauge|histogram|summary)
        foreach (var typeLine in typeLines)
        {
            Assert.Matches(@"^# TYPE\s+[a-zA-Z_:][a-zA-Z0-9_:]*\s+(counter|gauge|histogram|summary|untyped)$", typeLine);
        }
    }

    [Fact]
    public async Task MetricsEndpoint_SupportsHttpGet()
    {
        // Act
        var response = await Client.GetAsync("/career/metrics");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task MetricsEndpoint_HandlesHttpPost()
    {
        // Act - POST should still work (body ignored, returns same metrics)
        var response = await Client.PostAsync("/career/metrics", new StringContent(string.Empty));

        // Assert - Prometheus scraper endpoints typically accept any HTTP method
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task MetricsEndpoint_IsIdempotent()
    {
        // Act - Call twice
        var response1 = await Client.GetAsync("/career/metrics");
        var content1 = await response1.Content.ReadAsStringAsync();

        await Task.Delay(100); // Small delay

        var response2 = await Client.GetAsync("/career/metrics");
        var content2 = await response2.Content.ReadAsStringAsync();

        // Assert - Both should succeed
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);

        // Content should have same structure (though values may differ)
        Assert.Contains("# HELP", content1);
        Assert.Contains("# HELP", content2);

        var lines1 = content1.Split('\n').Where(l => l.StartsWith("# HELP")).ToList();
        var lines2 = content2.Split('\n').Where(l => l.StartsWith("# HELP")).ToList();

        // At least some metric definitions should be present
        Assert.NotEmpty(lines1);
        Assert.NotEmpty(lines2);
        
        // Idempotency: the set of metrics should be consistent
        // We allow some jitter if metrics are dynamically registered, but core ones should remain
        Assert.True(Math.Abs(lines1.Count - lines2.Count) <= 5, $"Metrics count changed significantly: {lines1.Count} -> {lines2.Count}");
    }

}
