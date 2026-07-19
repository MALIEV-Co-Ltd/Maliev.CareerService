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
            services.RemoveAll<Application.Services.External.IUploadServiceClient>();
            services.AddSingleton<Application.Services.External.IUploadServiceClient, Mocks.MockUploadServiceClient>();

            services.RemoveAll<Application.Services.External.IEmailServiceClient>();
            services.AddSingleton<Application.Services.External.IEmailServiceClient, Mocks.MockEmailServiceClient>();

            services.RemoveAll<Application.Services.External.ICountryServiceClient>();
            services.AddSingleton<Application.Services.External.ICountryServiceClient, Mocks.MockCountryServiceClient>();

            services.RemoveAll<Application.Services.External.IEmployeeServiceClient>();
            services.AddSingleton<Application.Services.External.IEmployeeServiceClient, Mocks.MockEmployeeServiceClient>();

            services.RemoveAll<Application.Services.External.INotificationServiceClient>();
            services.AddSingleton<Application.Services.External.INotificationServiceClient, Mocks.MockNotificationServiceClient>();

            services.AddMassTransitTestHarness();
        });
    }
}
