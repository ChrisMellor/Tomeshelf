using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using Tomeshelf.Application.Options;
using Tomeshelf.Infrastructure.Domains.Bundles.Queries;
using Tomeshelf.Infrastructure.Domains.Bundles.Services;
using Tomeshelf.Infrastructure.Domains.Fitness.Clients;
using Tomeshelf.Infrastructure.Domains.Fitness.Services;
using Tomeshelf.Infrastructure.Domains.Guests.Clients;
using Tomeshelf.Infrastructure.Domains.Guests.Queries;
using Tomeshelf.Infrastructure.Domains.Guests.Services;
using Tomeshelf.Infrastructure.Domains.Upload;
using Tomeshelf.Infrastructure.Domains.Upload.Factories;
using Tomeshelf.Infrastructure.Domains.Upload.Services;

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

    /// <summary>
    ///     Registers only the upload pipeline (no DB dependencies) for the file uploader API.
    /// </summary>
    public static IServiceCollection AddBundleUploadInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<BundleFileOrganiser>();
        services.AddSingleton<IGoogleDriveClientFactory, GoogleDriveClientFactory>();
        services.AddScoped<IBundleUploadService, BundleUploadService>();

        return services;
    }

    public static IServiceCollection AddFitnessInfrastructure(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddHttpContextAccessor();
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
}