using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Tomeshelf.Executor.Jobs;
using Tomeshelf.Executor.Services;

namespace Tomeshelf.Executor.Tests;

public class ProgramTests
{
    [Fact]
    public void BuildApp_RegistersExecutorServices()
    {
        using var app = Program.BuildApp(Array.Empty<string>(), builder =>
        {
            builder.Logging.ClearProviders();
        });

        var services = app.Services;
        services.GetRequiredService<IExecutorConfigurationStore>()
                .Should()
                .NotBeNull();
        services.GetRequiredService<IExecutorSchedulerOrchestrator>()
                .Should()
                .NotBeNull();
        services.GetRequiredService<IApiEndpointDiscoveryService>()
                .Should()
                .NotBeNull();
        services.GetRequiredService<IEndpointPingService>()
                .Should()
                .NotBeNull();

        services.GetServices<IHostedService>()
                .Should()
                .ContainSingle(service => service is ExecutorSchedulerHostedService);

        var factory = services.GetRequiredService<IHttpClientFactory>();
        factory.CreateClient(TriggerEndpointJob.HttpClientName)
               .Should()
               .NotBeNull();
        factory.CreateClient(ApiEndpointDiscoveryService.HttpClientName)
               .Should()
               .NotBeNull();
    }
}