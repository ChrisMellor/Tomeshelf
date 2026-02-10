using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shouldly;
using Tomeshelf.HumbleBundle.Application.Abstractions.External;
using Tomeshelf.HumbleBundle.Application.Abstractions.Persistence;
using Tomeshelf.HumbleBundle.Infrastructure.Bundles;

namespace Tomeshelf.HumbleBundle.Infrastructure.Tests.DependencyInjectionTests;

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
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?> { ["ConnectionStrings:humblebundledb"] = "Server=(localdb)\\mssqllocaldb;Database=HumbleBundleTest;Trusted_Connection=True;" });

        // Act
        builder.AddInfrastructureServices();

        // Assert
        using var provider = builder.Services.BuildServiceProvider();
        provider.GetRequiredService<TomeshelfBundlesDbContext>()
                .ShouldNotBeNull();
        provider.GetRequiredService<IHumbleBundleScraper>()
                .ShouldBeOfType<HumbleBundleScraper>();
        provider.GetRequiredService<IBundleQueries>()
                .ShouldBeOfType<BundleQueries>();
        provider.GetRequiredService<IBundleIngestService>()
                .ShouldBeOfType<BundleIngestService>();

        var factory = provider.GetRequiredService<IHttpClientFactory>();
        var client = factory.CreateClient(HumbleBundleScraper.HttpClientName);
        client.Timeout.ShouldBe(TimeSpan.FromSeconds(30));
        client.DefaultRequestHeaders
              .UserAgent
              .ToString()
              .ShouldContain("Tomeshelf-HumbleBundle/1.0");
    }

    /// <summary>
    ///     Throws the when connection string missing.
    /// </summary>
    [Fact]
    public void ThrowsWhenConnectionStringMissing()
    {
        // Arrange
        var builder = new HostApplicationBuilder();

        // Act
        var exception = Should.Throw<InvalidOperationException>(() => builder.AddInfrastructureServices());

        // Assert
        exception.Message.ShouldBe("Connection string 'humblebundledb' is missing.");
    }
}