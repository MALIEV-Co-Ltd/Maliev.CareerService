using System.Net;
using Maliev.CareerService.Api.Services.External;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Polly;
using Xunit;

namespace Maliev.CareerService.Tests.Integration.Notifications;

public class NotificationRetryTests
{
    [Fact]
    public async Task NotificationServiceClient_ShouldRetryOnFailure()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        int callCount = 0;

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(() =>
            {
                callCount++;
                if (callCount < 3)
                {
                    return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
                }
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{}")
                };
            });

        // Create a resilience pipeline with retry
        var retryPipeline = new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddRetry(new HttpRetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                BackoffType = DelayBackoffType.Constant,
                Delay = TimeSpan.FromMilliseconds(10)
            })
            .Build();

        // Create HttpClient with retry handler
        var resilienceHandler = new ResilienceHandler(retryPipeline)
        {
            InnerHandler = handlerMock.Object
        };

        var httpClient = new HttpClient(resilienceHandler)
        {
            BaseAddress = new Uri("http://notification-service")
        };

        var loggerMock = new Mock<ILogger<NotificationServiceClient>>();
        var service = new NotificationServiceClient(httpClient, loggerMock.Object);

        // Act
        await service.SendCertificationReminderAsync(
            Guid.NewGuid(), "Test Course", DateTime.UtcNow.AddDays(30), 30);

        // Assert - Should have retried and succeeded on the 3rd attempt
        Assert.Equal(3, callCount);
    }
}
