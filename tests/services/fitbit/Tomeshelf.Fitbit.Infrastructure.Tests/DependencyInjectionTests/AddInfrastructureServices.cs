using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tomeshelf.Fitbit.Application.Abstractions.Services;
using Tomeshelf.Fitbit.Infrastructure;

namespace Tomeshelf.Fitbit.Infrastructure.Tests.DependencyInjectionTests;

public class AddInfrastructureServices
{
    [Fact]
    public async Task RegistersExpectedServices()
    {
        // Arrange
        var builder = new HostApplicationBuilder();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:fitbitDb"] = @"Server=(localdb)\mssqllocaldb;Database=FitbitTest;Trusted_Connection=True;",
            ["Fitbit:ApiBase"] = "https://fitbit.example/"
        });

        // Act
        builder.AddInfrastructureServices();

        // Assert
        await using var provider = builder.Services.BuildServiceProvider();

        provider.GetRequiredService<IFitbitAuthorizationService>().ShouldBeOfType<FitbitAuthorizationService>();
        provider.GetRequiredService<IFitbitDashboardService>().ShouldBeOfType<FitbitDashboardService>();
        provider.GetRequiredService<IFitbitOverviewService>().ShouldBeOfType<FitbitOverviewService>();
        provider.GetRequiredService<IFitbitTokenCache>().ShouldBeOfType<FitbitTokenCache>();

        var apiClientService = provider.GetRequiredService<IFitbitApiClient>();
        apiClientService.ShouldNotBeNull();
        apiClientService.GetType().Name.ShouldBe("FitbitApiClient");

        var factory = provider.GetRequiredService<IHttpClientFactory>();
        var apiClient = factory.CreateClient("Fitbit.ApiClient");
        apiClient.BaseAddress.ShouldBe(new Uri("https://fitbit.example/"));
        apiClient.DefaultRequestHeaders.UserAgent.ToString().ShouldContain("Tomeshelf-FitbitApi/1.0");

        var authClient = factory.CreateClient(FitbitAuthorizationService.HttpClientName);
        authClient.BaseAddress.ShouldBe(new Uri("https://fitbit.example/"));
    }
}
