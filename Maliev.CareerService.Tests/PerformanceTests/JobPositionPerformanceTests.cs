using System.Diagnostics;
using Maliev.CareerService.Tests.IntegrationTests;
using Xunit;
using Xunit.Abstractions;

namespace Maliev.CareerService.Tests.PerformanceTests;

/// <summary>
/// Performance tests to measure API response times and throughput.
/// These tests help identify performance bottlenecks and ensure the API can handle expected load.
/// </summary>
public class JobPositionPerformanceTests : IClassFixture<CareerServiceWebApplicationFactory>
{
    private readonly CareerServiceWebApplicationFactory _factory;
    private readonly ITestOutputHelper _output;

    public JobPositionPerformanceTests(CareerServiceWebApplicationFactory factory, ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;
    }

    [Fact]
    public async Task GetJobPositions_PerformanceTest_100ConcurrentRequests()
    {
        // Arrange
        var client = _factory.CreateClient();
        await _factory.ClearDatabaseAsync();

        var stopwatch = Stopwatch.StartNew();

        // Act - Send 100 concurrent requests
        var tasks = new List<Task>();
        for (int i = 0; i < 100; i++)
        {
            tasks.Add(client.GetAsync("/careers/v1.0/positions/search"));
        }

        await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        var averageResponseTime = stopwatch.ElapsedMilliseconds / 100.0;
        _output.WriteLine($"Average response time for 100 concurrent requests: {averageResponseTime} ms");
        
        // Performance goal: Average response time should be less than 1000ms
        Assert.True(averageResponseTime < 1000, $"Average response time {averageResponseTime}ms exceeds performance goal of 1000ms");
    }

    [Fact]
    public async Task GetJobPositions_PerformanceTest_SequentialRequests()
    {
        // Arrange
        var client = _factory.CreateClient();
        await _factory.ClearDatabaseAsync();

        var stopwatch = Stopwatch.StartNew();

        // Act - Send 100 sequential requests
        for (int i = 0; i < 100; i++)
        {
            var response = await client.GetAsync("/careers/v1.0/positions/search");
            Assert.True(response.IsSuccessStatusCode);
        }

        stopwatch.Stop();

        // Assert
        var totalTime = stopwatch.ElapsedMilliseconds;
        var averageResponseTime = totalTime / 100.0;
        _output.WriteLine($"Average response time for 100 sequential requests: {averageResponseTime} ms");
        _output.WriteLine($"Total time for 100 requests: {totalTime} ms");
        
        // Performance goal: Average response time should be less than 500ms
        Assert.True(averageResponseTime < 500, $"Average response time {averageResponseTime}ms exceeds performance goal of 500ms");
    }
}