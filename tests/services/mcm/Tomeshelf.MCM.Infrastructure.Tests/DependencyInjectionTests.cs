using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tomeshelf.MCM.Application.Abstractions.Clients;
using Tomeshelf.MCM.Application.Abstractions.Persistence;
using Tomeshelf.MCM.Infrastructure;
using Tomeshelf.MCM.Infrastructure.Clients;
using Tomeshelf.MCM.Infrastructure.Repositories;

namespace Tomeshelf.MCM.Infrastructure.Tests;

public class DependencyInjectionTests
{
    [Fact]
    public void AddInfrastructureServices_ThrowsWhenConnectionStringMissing()
    {
        var builder = new HostApplicationBuilder();

        Action act = () => builder.AddInfrastructureServices();

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("Connection string 'mcmdb' is missing.");
    }

    [Fact]
    public void AddInfrastructureServices_RegistersExpectedServices()
    {
        var builder = new HostApplicationBuilder();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:mcmdb"] = "Server=(localdb)\\mssqllocaldb;Database=McmTest;Trusted_Connection=True;"
        });

        builder.AddInfrastructureServices();

        using var provider = builder.Services.BuildServiceProvider();
        provider.GetRequiredService<TomeshelfMcmDbContext>().Should().NotBeNull();
        provider.GetRequiredService<IEventRepository>().Should().BeOfType<EventRepository>();
        provider.GetRequiredService<IGuestsRepository>().Should().BeOfType<GuestsRepository>();
        provider.GetRequiredService<IMcmGuestsClient>().Should().BeOfType<McmGuestsClient>();

        var factory = provider.GetRequiredService<IHttpClientFactory>();
        var client = factory.CreateClient(McmGuestsClient.HttpClientName);

        client.BaseAddress.Should().Be(new Uri("https://conventions.leapevent.tech/"));
        client.Timeout.Should().Be(TimeSpan.FromSeconds(30));
        client.DefaultRequestHeaders.UserAgent.ToString().Should().Contain("Tomeshelf-McmApi/1.0");
    }
}
