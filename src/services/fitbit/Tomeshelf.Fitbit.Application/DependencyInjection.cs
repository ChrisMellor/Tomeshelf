using Microsoft.Extensions.DependencyInjection;
using Tomeshelf.Fitbit.Application.Abstractions.Messaging;
using Tomeshelf.Fitbit.Application.Features.Authorization.Commands;
using Tomeshelf.Fitbit.Application.Features.Authorization.Models;
using Tomeshelf.Fitbit.Application.Features.Authorization.Queries;
using Tomeshelf.Fitbit.Application.Features.Dashboard.Queries;

namespace Tomeshelf.Fitbit.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IQueryHandler<GetFitbitDashboardQuery, FitbitDashboardDto>, GetFitbitDashboardQueryHandler>();
        services.AddScoped<ICommandHandler<BuildFitbitAuthorizationRedirectCommand, FitbitAuthorizationRedirect>, BuildFitbitAuthorizationRedirectCommandHandler>();
        services.AddScoped<ICommandHandler<ExchangeFitbitAuthorizationCodeCommand, FitbitAuthorizationExchangeResult>, ExchangeFitbitAuthorizationCodeCommandHandler>();
        services.AddScoped<IQueryHandler<GetFitbitAuthorizationStatusQuery, FitbitAuthorizationStatus>, GetFitbitAuthorizationStatusQueryHandler>();

        return services;
    }
}
