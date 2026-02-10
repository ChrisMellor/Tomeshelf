using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tomeshelf.Paissa.Application.Abstractions.Common;
using Tomeshelf.Paissa.Application.Abstractions.External;
using Tomeshelf.Paissa.Infrastructure.Services;
using Tomeshelf.Paissa.Infrastructure.Services.External;
using Tomeshelf.Paissa.Infrastructure.Settings;

namespace Tomeshelf.Paissa.Infrastructure;

/// <summary>
///     Provides extension methods for registering infrastructure services required by the application.
/// </summary>
/// <remarks>
///     This class is intended to be used during application startup to configure dependencies such as HTTP
///     clients and application settings. By calling these methods, the necessary services are added to the dependency
///     injection container, ensuring they are available throughout the application's lifetime.
/// </remarks>
public static class DependencyInjection
{
    /// <summary>
    ///     Adds the infrastructure services.
    /// </summary>
    /// <param name="builder">The builder.</param>
    public static void AddInfrastructureServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddScoped<IPaissaClient, PaissaClient>();
        builder.Services.AddHttpClient(PaissaClient.HttpClientName, client =>
        {
            client.BaseAddress = new Uri("https://paissadb.zhu.codes/");
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Tomeshelf-PaissaApi/1.0");
        });

        var worldId = builder.Configuration.GetValue("Paissa:WorldId", 33);
        builder.Services.AddSingleton<IPaissaWorldSettings>(new PaissaWorldSettings(worldId));
        builder.Services.AddSingleton<IClock, SystemClock>();
    }
}