using FakeItEasy;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Tomeshelf.Application.Shared.Abstractions.Messaging;
using Tomeshelf.HumbleBundle.Application.Abstractions.External;
using Tomeshelf.HumbleBundle.Application.Abstractions.Persistence;
using Tomeshelf.HumbleBundle.Application.Features.Bundles.Commands;
using Tomeshelf.HumbleBundle.Application.Features.Bundles.Models;
using Tomeshelf.HumbleBundle.Application.Features.Bundles.Queries;
using Tomeshelf.HumbleBundle.Application.HumbleBundle;

namespace Tomeshelf.HumbleBundle.Application.Tests.DependencyInjectionTests;

public class AddApplicationServices
{
    [Fact]
    public void RegistersHandlers()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton(A.Fake<IBundleQueries>());
        services.AddSingleton(A.Fake<IHumbleBundleScraper>());
        services.AddSingleton(A.Fake<IBundleIngestService>());

        // Act
        services.AddApplicationServices();

        // Assert
        using var provider = services.BuildServiceProvider();
        provider.GetRequiredService<IQueryHandler<GetBundlesQuery, IReadOnlyList<BundleDto>>>()
                .ShouldBeOfType<GetBundlesQueryHandler>();
        provider.GetRequiredService<ICommandHandler<RefreshBundlesCommand, BundleIngestResult>>()
                .ShouldBeOfType<RefreshBundlesCommandHandler>();
    }
}