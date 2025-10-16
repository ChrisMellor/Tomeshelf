using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Tomeshelf.Application.Options;
using Tomeshelf.Infrastructure.Bundles;
using Tomeshelf.Infrastructure.Clients;
using Tomeshelf.Infrastructure.Fitness;
using Tomeshelf.Infrastructure.Persistence;
using Tomeshelf.Infrastructure.Queries;
using Tomeshelf.Infrastructure.Services;

namespace Tomeshelf.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<GuestQueries>();
        services.AddScoped<EventIngestService>();
        services.AddHttpClient<IGuestsClient, GuestsClient>();
        services.AddTransient<IGuestService, GuestService>();

        return services;
    }

    public static IServiceCollection AddBundleInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<BundleQueries>();
        services.AddScoped<BundleIngestService>();
        services.AddHttpClient<IHumbleBundleScraper, HumbleBundleScraper>(client =>
        {
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Tomeshelf/1.0 (+https://github.com/ChrisMellor/Tomeshelf)");
        });

        return services;
    }

    public static IServiceCollection AddFitnessInfrastructure(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddSingleton<FitbitTokenCache>();
        services.AddHttpClient<IFitbitApiClient, FitbitApiClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptionsMonitor<FitbitOptions>>()
                            .CurrentValue;
            var baseUrl = string.IsNullOrWhiteSpace(options.ApiBase)
                    ? "https://api.fitbit.com/"
                    : options.ApiBase;

            if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri))
            {
                throw new InvalidOperationException($"Invalid Fitbit API base URL '{baseUrl}'.");
            }

            client.BaseAddress = uri;
            client.Timeout = TimeSpan.FromMinutes(2);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Tomeshelf/1.0 (+https://github.com/ChrisMellor/Tomeshelf)");
        });
        services.AddHttpClient("FitbitOAuth", (sp, client) =>
        {
            var options = sp.GetRequiredService<IOptionsMonitor<FitbitOptions>>()
                            .CurrentValue;
            var baseUrl = string.IsNullOrWhiteSpace(options.ApiBase)
                    ? "https://api.fitbit.com/"
                    : options.ApiBase;

            if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri))
            {
                throw new InvalidOperationException($"Invalid Fitbit API base URL '{baseUrl}'.");
            }

            client.BaseAddress = uri;
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Tomeshelf/1.0 (+https://github.com/ChrisMellor/Tomeshelf)");
        });
        services.AddTransient<FitbitDashboardService>();
        services.AddSingleton<FitbitAuthorizationService>();

        return services;
    }

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<TomeshelfComicConDbContext>(options => options.UseSqlServer(connectionString));

        return services.AddInfrastructure();
    }
}
