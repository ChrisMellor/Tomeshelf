using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shouldly;
using Tomeshelf.MCM.Application.Abstractions.Clients;
using Tomeshelf.MCM.Application.Abstractions.Persistence;
using Tomeshelf.MCM.Infrastructure.Clients;
using Tomeshelf.MCM.Infrastructure.Repositories;

namespace Tomeshelf.MCM.Infrastructure.Tests.DependencyInjectionTests;

public class AddInfrastructureServices
{
    [Fact]
    public void RegistersExpectedServices()
    {
        var builder = new HostApplicationBuilder();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?> { ["ConnectionStrings:mcmdb"] = @"Server=(localdb)\mssqllocaldb;Database=McmTest;Trusted_Connection=True;" });

        builder.AddInfrastructureServices();

        using var provider = builder.Services.BuildServiceProvider();
        provider.GetRequiredService<TomeshelfMcmDbContext>()
                .ShouldNotBeNull();
        provider.GetRequiredService<IEventRepository>()
                .ShouldBeOfType<EventRepository>();
        provider.GetRequiredService<IGuestsRepository>()
                .ShouldBeOfType<GuestsRepository>();
        provider.GetRequiredService<IMcmGuestsClient>()
                .ShouldBeOfType<McmGuestsClient>();

        var factory = provider.GetRequiredService<IHttpClientFactory>();
        var client = factory.CreateClient(McmGuestsClient.HttpClientName);
        client.BaseAddress.ShouldBe(new Uri("https://conventions.leapevent.tech/"));
        client.Timeout.ShouldBe(TimeSpan.FromSeconds(30));
        client.DefaultRequestHeaders
              .UserAgent
              .ToString()
              .ShouldContain("Tomeshelf-McmApi/1.0");
    }

    [Fact]
    public void ThrowsWhenConnectionStringMissing()
    {
        var builder = new HostApplicationBuilder();
        builder.Configuration.Sources.Clear();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>());

        var exception = Should.Throw<InvalidOperationException>(() => builder.AddInfrastructureServices());

        exception.Message.ShouldBe("Connection string 'mcmdb' is missing.");
    }
}