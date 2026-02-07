using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shouldly;
using Tomeshelf.Executor.Jobs;
using Tomeshelf.Executor.Services;

namespace Tomeshelf.Executor.Tests.ProgramTests;

public class BuildApp
{
    [Fact]
    public void RegistersExecutorServices()
    {
        using var app = Program.BuildApp(Array.Empty<string>(), builder =>
        {
            builder.Logging.ClearProviders();
        });

        var services = app.Services;

        services.GetRequiredService<IExecutorConfigurationStore>()
                .ShouldNotBeNull();
        services.GetRequiredService<IExecutorSchedulerOrchestrator>()
                .ShouldNotBeNull();
        services.GetRequiredService<IApiEndpointDiscoveryService>()
                .ShouldNotBeNull();
        services.GetRequiredService<IEndpointPingService>()
                .ShouldNotBeNull();

        var hosted = services.GetServices<IHostedService>()
                             .Single(service => service is ExecutorSchedulerHostedService);
        hosted.ShouldNotBeNull();

        var factory = services.GetRequiredService<IHttpClientFactory>();
        factory.CreateClient(TriggerEndpointJob.HttpClientName)
               .ShouldNotBeNull();
        factory.CreateClient(ApiEndpointDiscoveryService.HttpClientName)
               .ShouldNotBeNull();
    }
}