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
    /// <summary>
    ///     Registers the expected services.
    /// </summary>
    [Fact]
    public void RegistersExpectedServices()
    {
        // Arrange
        var builder = new HostApplicationBuilder();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?> { ["ConnectionStrings:mcmdb"] = @"Server=(localdb)\mssqllocaldb;Database=McmTest;Trusted_Connection=True;" });

        // Act
        builder.AddInfrastructureServices();

        // Assert
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

    /// <summary>
    ///     Throws the when connection string missing.
    /// </summary>
    [Fact]
    public void ThrowsWhenConnectionStringMissing()
    {
        // Arrange
        var builder = new HostApplicationBuilder();
        builder.Configuration.Sources.Clear();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>());

        // Act
        var exception = Should.Throw<InvalidOperationException>(() => builder.AddInfrastructureServices());

        // Assert
        exception.Message.ShouldBe("Connection string 'mcmdb' is missing.");
    }
}