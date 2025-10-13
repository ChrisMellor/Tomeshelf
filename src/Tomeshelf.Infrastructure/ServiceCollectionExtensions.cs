using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tomeshelf.Infrastructure.Bundles;
using Tomeshelf.Infrastructure.Clients;
using Tomeshelf.Infrastructure.Persistence;
using Tomeshelf.Infrastructure.Queries;
using Tomeshelf.Infrastructure.Services;

namespace Tomeshelf.Infrastructure;

public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Registers infrastructure services assuming the DbContext is configured elsewhere (Aspire-friendly overload).
    /// </summary>
    /// <param name="services">The DI service collection.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<GuestQueries>();
        services.AddScoped<EventIngestService>();
        services.AddHttpClient<IGuestsClient, GuestsClient>();
        services.AddTransient<IGuestService, GuestService>();
        return services;
    }

    /// <summary>
    ///     Registers Humble Bundle scraping and query services.
    /// </summary>
    /// <param name="services">The DI service collection.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddBundleInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<BundleQueries>();
        services.AddScoped<BundleIngestService>();
        services.AddHttpClient<IHumbleBundleScraper, HumbleBundleScraper>(client =>
        {
            client.DefaultRequestHeaders.UserAgent.ParseAdd(
                "Tomeshelf/1.0 (+https://github.com/ChrisMellor/Tomeshelf)");
        });

        return services;
    }

    /// <summary>
    ///     Registers infrastructure services and configures the EF Core DbContext (classic overload).
    /// </summary>
    /// <param name="services">The DI service collection.</param>
    /// <param name="connectionString">SQL Server connection string.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<TomeshelfComicConDbContext>(options => options.UseSqlServer(connectionString));
        return services.AddInfrastructure();
    }
}