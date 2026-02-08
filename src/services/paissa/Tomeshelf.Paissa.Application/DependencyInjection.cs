using Microsoft.Extensions.DependencyInjection;
using Tomeshelf.Application.Shared.Abstractions.Messaging;
using Tomeshelf.Paissa.Application.Features.Housing.Dtos;
using Tomeshelf.Paissa.Application.Features.Housing.Queries;

namespace Tomeshelf.Paissa.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IQueryHandler<GetAcceptingEntriesQuery, PaissaWorldSummaryDto>, GetAcceptingEntriesQueryHandler>();

        return services;
    }
}