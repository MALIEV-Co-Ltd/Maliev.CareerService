using Maliev.CareerService.Infrastructure.Data;
using Maliev.CareerService.Tests.Testing;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Maliev.CareerService.Tests.Factories;

public class TestWebApplicationFactory : BaseIntegrationTestFactory<Program, CareerDbContext>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureTestServices(services =>
        {
            // Replace external services with mocks for all tests
            services.RemoveAll<Api.Services.External.IUploadServiceClient>();
            services.AddSingleton<Api.Services.External.IUploadServiceClient, Mocks.MockUploadServiceClient>();

            services.RemoveAll<Api.Services.External.IEmailServiceClient>();
            services.AddSingleton<Api.Services.External.IEmailServiceClient, Mocks.MockEmailServiceClient>();

            services.RemoveAll<Api.Services.External.ICountryServiceClient>();
            services.AddSingleton<Api.Services.External.ICountryServiceClient, Mocks.MockCountryServiceClient>();

            services.RemoveAll<Api.Services.External.IEmployeeServiceClient>();
            services.AddSingleton<Api.Services.External.IEmployeeServiceClient, Mocks.MockEmployeeServiceClient>();

            services.RemoveAll<Api.Services.External.INotificationServiceClient>();
            services.AddSingleton<Api.Services.External.INotificationServiceClient, Mocks.MockNotificationServiceClient>();

            services.AddMassTransitTestHarness();
        });
    }
}
