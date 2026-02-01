using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Tomeshelf.Application.Shared.Abstractions.Messaging;
using Tomeshelf.HumbleBundle.Application.Features.Bundles.Commands;
using Tomeshelf.HumbleBundle.Application.Features.Bundles.Models;
using Tomeshelf.HumbleBundle.Application.Features.Bundles.Queries;
using Tomeshelf.HumbleBundle.Application.HumbleBundle;

namespace Tomeshelf.HumbleBundle.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IQueryHandler<GetBundlesQuery, IReadOnlyList<BundleDto>>, GetBundlesQueryHandler>();
        services.AddScoped<ICommandHandler<RefreshBundlesCommand, BundleIngestResult>, RefreshBundlesCommandHandler>();

        return services;
    }
}
