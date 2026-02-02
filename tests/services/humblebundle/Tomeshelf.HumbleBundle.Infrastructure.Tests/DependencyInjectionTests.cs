using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tomeshelf.HumbleBundle.Application.Abstractions.External;
using Tomeshelf.HumbleBundle.Application.Abstractions.Persistence;
using Tomeshelf.HumbleBundle.Infrastructure;
using Tomeshelf.HumbleBundle.Infrastructure.Bundles;

namespace Tomeshelf.HumbleBundle.Infrastructure.Tests;

public class DependencyInjectionTests
{
    [Fact]
    public void AddInfrastructureServices_ThrowsWhenConnectionStringMissing()
    {
        var builder = new HostApplicationBuilder();

        Action act = () => builder.AddInfrastructureServices();

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("Connection string 'humblebundledb' is missing.");
    }

    [Fact]
    public void AddInfrastructureServices_RegistersExpectedServices()
    {
        var builder = new HostApplicationBuilder();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:humblebundledb"] = "Server=(localdb)\\mssqllocaldb;Database=HumbleBundleTest;Trusted_Connection=True;"
        });

        builder.AddInfrastructureServices();

        using var provider = builder.Services.BuildServiceProvider();
        provider.GetRequiredService<TomeshelfBundlesDbContext>().Should().NotBeNull();
        provider.GetRequiredService<IHumbleBundleScraper>().Should().BeOfType<HumbleBundleScraper>();
        provider.GetRequiredService<IBundleQueries>().Should().BeOfType<BundleQueries>();
        provider.GetRequiredService<IBundleIngestService>().Should().BeOfType<BundleIngestService>();

        var factory = provider.GetRequiredService<IHttpClientFactory>();
        var client = factory.CreateClient(HumbleBundleScraper.HttpClientName);
        client.Timeout.Should().Be(TimeSpan.FromSeconds(30));
        client.DefaultRequestHeaders.UserAgent.ToString().Should().Contain("Tomeshelf-HumbleBundle/1.0");
    }
}
