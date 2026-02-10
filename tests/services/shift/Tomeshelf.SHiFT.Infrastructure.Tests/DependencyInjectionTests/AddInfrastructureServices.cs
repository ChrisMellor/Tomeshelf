using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shouldly;
using Tomeshelf.SHiFT.Application.Abstractions.Common;
using Tomeshelf.SHiFT.Application.Abstractions.External;
using Tomeshelf.SHiFT.Application.Abstractions.Persistence;
using Tomeshelf.SHiFT.Application.Abstractions.Security;
using Tomeshelf.SHiFT.Infrastructure.Exceptions;
using Tomeshelf.SHiFT.Infrastructure.Persistence;
using Tomeshelf.SHiFT.Infrastructure.Persistence.Repositories;
using Tomeshelf.SHiFT.Infrastructure.Security;
using Tomeshelf.SHiFT.Infrastructure.Services;
using Tomeshelf.SHiFT.Infrastructure.Services.External;

namespace Tomeshelf.SHiFT.Infrastructure.Tests.DependencyInjectionTests;

public class AddInfrastructureServices
{
    /// <summary>
    ///     Registers the expected services.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task RegistersExpectedServices()
    {
        // Arrange
        var builder = new HostApplicationBuilder();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?> { ["ConnectionStrings:shiftdb"] = @"Server=(localdb)\mssqllocaldb;Database=ShiftTest;Trusted_Connection=True;" });

        builder.AddInfrastructureServices();

        // Act
        await using var provider = builder.Services.BuildServiceProvider();

        // Assert
        provider.GetRequiredService<TomeshelfShiftDbContext>()
                .ShouldNotBeNull();
        provider.GetRequiredService<IShiftSettingsRepository>()
                .ShouldBeOfType<ShiftSettingsRepository>();
        provider.GetRequiredService<IShiftWebSession>()
                .ShouldBeOfType<ShiftWebSession>();
        provider.GetRequiredService<IShiftWebSessionFactory>()
                .ShouldBeOfType<ShiftWebSessionFactory>();
        provider.GetRequiredService<IGearboxClient>()
                .ShouldBeOfType<GearboxClient>();
        provider.GetRequiredService<IShiftKeySource>()
                .ShouldBeOfType<XShiftKeySource>();
        provider.GetRequiredService<ISecretProtector>()
                .ShouldBeOfType<DataProtectionSecretProtector>();
        provider.GetRequiredService<IClock>()
                .ShouldBeOfType<SystemClock>();

        var factory = provider.GetRequiredService<IHttpClientFactory>();
        var shiftClient = factory.CreateClient(ShiftWebSession.HttpClientName);
        shiftClient.BaseAddress.ShouldBe(new Uri("https://shift.gearboxsoftware.com/"));

        var xClient = factory.CreateClient(XShiftKeySource.HttpClientName);
        xClient.DefaultRequestHeaders
               .Accept
               .ToString()
               .ShouldContain("application/json");

        var authClient = factory.CreateClient(XAppOnlyTokenProvider.HttpClientName);
        authClient.DefaultRequestHeaders
                  .Accept
                  .ToString()
                  .ShouldContain("application/json");
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
        var action = () => builder.AddInfrastructureServices();

        // Assert
        Should.Throw<MissingConnectionStringException>(action);
    }
}