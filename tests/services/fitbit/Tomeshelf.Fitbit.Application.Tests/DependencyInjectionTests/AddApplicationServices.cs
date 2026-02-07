using FakeItEasy;
using Microsoft.Extensions.DependencyInjection;
using Tomeshelf.Application.Shared.Abstractions.Messaging;
using Tomeshelf.Fitbit.Application;
using Tomeshelf.Fitbit.Application.Abstractions.Services;
using Tomeshelf.Fitbit.Application.Features.Authorization.Commands;
using Tomeshelf.Fitbit.Application.Features.Authorization.Models;
using Tomeshelf.Fitbit.Application.Features.Authorization.Queries;
using Tomeshelf.Fitbit.Application.Features.Dashboard.Queries;
using Tomeshelf.Fitbit.Application.Features.Overview.Models;
using Tomeshelf.Fitbit.Application.Features.Overview.Queries;

namespace Tomeshelf.Fitbit.Application.Tests.DependencyInjectionTests;

public class AddApplicationServices
{
    [Fact]
    public void RegistersHandlers()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton(A.Fake<IFitbitDashboardService>());
        services.AddSingleton(A.Fake<IFitbitOverviewService>());
        services.AddSingleton(A.Fake<IFitbitAuthorizationService>());
        services.AddSingleton(A.Fake<IFitbitTokenCache>());

        // Act
        services.AddApplicationServices();

        using var provider = services.BuildServiceProvider();

        // Assert
        provider.GetRequiredService<IQueryHandler<GetFitbitDashboardQuery, FitbitDashboardDto>>()
                .ShouldBeOfType<GetFitbitDashboardQueryHandler>();
        provider.GetRequiredService<IQueryHandler<GetFitbitOverviewQuery, FitbitOverviewDto>>()
                .ShouldBeOfType<GetFitbitOverviewQueryHandler>();
        provider.GetRequiredService<ICommandHandler<BuildFitbitAuthorizationRedirectCommand, FitbitAuthorizationRedirect>>()
                .ShouldBeOfType<BuildFitbitAuthorizationRedirectCommandHandler>();
        provider.GetRequiredService<ICommandHandler<ExchangeFitbitAuthorizationCodeCommand, FitbitAuthorizationExchangeResult>>()
                .ShouldBeOfType<ExchangeFitbitAuthorizationCodeCommandHandler>();
        provider.GetRequiredService<IQueryHandler<GetFitbitAuthorizationStatusQuery, FitbitAuthorizationStatus>>()
                .ShouldBeOfType<GetFitbitAuthorizationStatusQueryHandler>();
    }
}
