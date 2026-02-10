using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tomeshelf.Fitbit.Application.Abstractions.Services;

namespace Tomeshelf.Fitbit.Infrastructure;

public static class DependencyInjection
{
    private const string ConnectionName = "fitbitDb";

    /// <summary>
    ///     Adds the infrastructure services.
    /// </summary>
    /// <param name="builder">The builder.</param>
    public static void AddInfrastructureServices(this IHostApplicationBuilder builder)
    {
        builder.AddSqlServerDbContext<TomeshelfFitbitDbContext>(ConnectionName);

        builder.Services.AddMemoryCache();
        builder.Services.AddHttpContextAccessor();

        builder.Services.AddSingleton<FitbitTokenCache>();
        builder.Services.AddSingleton<IFitbitTokenCache>(sp => sp.GetRequiredService<FitbitTokenCache>());
        builder.Services.AddScoped<IFitbitAuthorizationService, FitbitAuthorizationService>();
        builder.Services.AddScoped<IFitbitDashboardService, FitbitDashboardService>();
        builder.Services.AddScoped<IFitbitOverviewService, FitbitOverviewService>();
        builder.Services.AddScoped<IFitbitApiClient, FitbitApiClient>();

        builder.Services.AddHttpClient(FitbitApiClient.HttpClientName, client =>
        {
            var apiBase = builder.Configuration.GetValue<string>("Fitbit:ApiBase");
            client.BaseAddress = new Uri(string.IsNullOrWhiteSpace(apiBase)
                                             ? "https://api.fitbit.com/"
                                             : apiBase);
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Tomeshelf-FitbitApi/1.0");
        });

        builder.Services.AddHttpClient(FitbitAuthorizationService.HttpClientName, client =>
        {
            var apiBase = builder.Configuration.GetValue<string>("Fitbit:ApiBase");
            client.BaseAddress = new Uri(string.IsNullOrWhiteSpace(apiBase)
                                             ? "https://api.fitbit.com/"
                                             : apiBase);
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Tomeshelf-FitbitApi/1.0");
        });
    }
}