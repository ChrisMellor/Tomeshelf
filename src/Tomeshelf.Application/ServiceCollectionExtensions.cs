using Microsoft.Extensions.DependencyInjection;

namespace Tomeshelf.Application;

/// <summary>
///     Dependency injection helpers for the application layer.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Registers application-layer services.
    /// </summary>
    /// <param name="services">The DI service collection.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        return services;
    }
}