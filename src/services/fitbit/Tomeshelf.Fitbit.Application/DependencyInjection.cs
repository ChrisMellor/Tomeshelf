using Microsoft.Extensions.DependencyInjection;
using Tomeshelf.Application.Shared.Abstractions.Messaging;
using Tomeshelf.Fitbit.Application.Features.Authorization.Commands;
using Tomeshelf.Fitbit.Application.Features.Authorization.Models;
using Tomeshelf.Fitbit.Application.Features.Authorization.Queries;
using Tomeshelf.Fitbit.Application.Features.Dashboard.Queries;
using Tomeshelf.Fitbit.Application.Features.Overview.Models;
using Tomeshelf.Fitbit.Application.Features.Overview.Queries;

namespace Tomeshelf.Fitbit.Application;

public static class DependencyInjection
{
    /// <summary>
    ///     Adds the application services.
    /// </summary>
    /// <param name="services">The services.</param>
    /// <returns>The result of the operation.</returns>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IQueryHandler<GetFitbitDashboardQuery, FitbitDashboardDto>, GetFitbitDashboardQueryHandler>();
        services.AddScoped<IQueryHandler<GetFitbitOverviewQuery, FitbitOverviewDto>, GetFitbitOverviewQueryHandler>();
        services.AddScoped<ICommandHandler<BuildFitbitAuthorizationRedirectCommand, FitbitAuthorizationRedirect>, BuildFitbitAuthorizationRedirectCommandHandler>();
        services.AddScoped<ICommandHandler<ExchangeFitbitAuthorizationCodeCommand, FitbitAuthorizationExchangeResult>, ExchangeFitbitAuthorizationCodeCommandHandler>();
        services.AddScoped<IQueryHandler<GetFitbitAuthorizationStatusQuery, FitbitAuthorizationStatus>, GetFitbitAuthorizationStatusQueryHandler>();

        return services;
    }
}