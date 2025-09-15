using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tomeshelf.Infrastructure.Persistence;

namespace Tomeshelf.Infrastructure;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers infrastructure services assuming the DbContext is configured elsewhere (Aspire-friendly overload).
    /// </summary>
    /// <param name="services">The DI service collection.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<Queries.GuestQueries>();
        services.AddScoped<Services.EventIngestService>();
        services.AddHttpClient<Clients.IGuestsClient, Clients.GuestsClient>();
        services.AddTransient<Services.IGuestService, Services.GuestService>();
        return services;
    }

    /// <summary>
    /// Registers infrastructure services and configures the EF Core DbContext (classic overload).
    /// </summary>
    /// <param name="services">The DI service collection.</param>
    /// <param name="connectionString">SQL Server connection string.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<TomeshelfDbContext>(options => options.UseSqlServer(connectionString));
        return services.AddInfrastructure();
    }
}
