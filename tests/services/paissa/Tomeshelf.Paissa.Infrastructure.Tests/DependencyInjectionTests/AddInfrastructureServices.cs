using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shouldly;
using Tomeshelf.Paissa.Application.Abstractions.Common;
using Tomeshelf.Paissa.Application.Abstractions.External;
using Tomeshelf.Paissa.Infrastructure.Services;
using Tomeshelf.Paissa.Infrastructure.Services.External;
using Tomeshelf.Paissa.Infrastructure.Settings;

namespace Tomeshelf.Paissa.Infrastructure.Tests.DependencyInjectionTests;

public class AddInfrastructureServices
{
    [Fact]
    public void RegistersExpectedServices()
    {
        // Arrange
        var builder = new HostApplicationBuilder();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?> { ["Paissa:WorldId"] = "99" });

        builder.AddInfrastructureServices();

        // Act
        using var provider = builder.Services.BuildServiceProvider();
        // Assert
        provider.GetRequiredService<IPaissaClient>()
                .ShouldBeOfType<PaissaClient>();

        var settings = provider.GetRequiredService<IPaissaWorldSettings>();
        settings.ShouldBeOfType<PaissaWorldSettings>();
        settings.WorldId.ShouldBe(99);

        provider.GetRequiredService<IClock>()
                .ShouldBeOfType<SystemClock>();

        var factory = provider.GetRequiredService<IHttpClientFactory>();
        var client = factory.CreateClient(PaissaClient.HttpClientName);
        client.BaseAddress.ShouldBe(new Uri("https://paissadb.zhu.codes/"));
    }
}