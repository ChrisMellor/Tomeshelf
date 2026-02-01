using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using Tomeshelf.SHiFT.Application.Abstractions.Common;
using Tomeshelf.SHiFT.Application.Abstractions.External;
using Tomeshelf.SHiFT.Application.Abstractions.Persistence;
using Tomeshelf.SHiFT.Application.Abstractions.Security;
using Tomeshelf.SHiFT.Infrastructure.Exceptions;
using Tomeshelf.SHiFT.Infrastructure.Persistence;
using Tomeshelf.SHiFT.Infrastructure.Persistence.Repositories;
using Tomeshelf.SHiFT.Infrastructure.Security;
using Tomeshelf.SHiFT.Infrastructure.Services;
using Tomeshelf.SHiFT.Infrastructure.Services.External;

namespace Tomeshelf.SHiFT.Infrastructure;

/// <summary>
///     Provides extension methods for configuring infrastructure-related dependency injection services for the
///     application.
/// </summary>
/// <remarks>
///     This class includes methods to register essential services such as the database context, data
///     protection, and application-specific services. It requires a valid connection string named "shiftdb" to be present
///     in the application's configuration. If the connection string is missing or empty, a
///     MissingConnectionStringException is thrown during setup.
/// </remarks>
public static class DependencyInjection
{
    private const string ConnectionName = "shiftdb";

    /// <summary>
    ///     Configures infrastructure services required for the application, including database context, data protection,
    ///     and service registrations.
    /// </summary>
    /// <remarks>
    ///     This method sets up the SQL Server database context and registers various services necessary
    ///     for the application's operation. Ensure that the connection string is correctly configured in the application's
    ///     settings before invoking this method.
    /// </remarks>
    /// <param name="builder">The host application builder used to configure services and middleware for the application.</param>
    /// <exception cref="MissingConnectionStringException">Thrown if the connection string for the database is null or empty.</exception>
    public static void AddInfrastructureServices(this IHostApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString(ConnectionName);

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new MissingConnectionStringException(connectionString);
        }

        builder.AddSqlServerDbContext<TomeshelfShiftDbContext>(ConnectionName);

        var dataProtection = builder.Services
                                    .AddDataProtection()
                                    //.SetApplicationName("Tomeshelf.SHiFT")
                                    .PersistKeysToDbContext<TomeshelfShiftDbContext>();

        builder.Services.AddScoped<IShiftSettingsRepository, ShiftSettingsRepository>();
        builder.Services.AddScoped<IShiftWebSession, ShiftWebSession>();
        builder.Services.AddSingleton<IShiftWebSessionFactory, ShiftWebSessionFactory>();
        builder.Services.AddScoped<IGearboxClient, GearboxClient>();
        builder.Services.AddScoped<IShiftKeySource, XShiftKeySource>();
        builder.Services.AddSingleton<XAppOnlyTokenProvider>();
        builder.Services.AddSingleton<ISecretProtector, DataProtectionSecretProtector>();
        builder.Services.AddSingleton<IClock, SystemClock>();

        builder.Services.AddHttpClient(ShiftWebSession.HttpClientName, client =>
        {
            client.BaseAddress = new Uri("https://shift.gearboxsoftware.com/");
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Tomeshelf-SHiFT/1.0");
        })
        .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
        {
            CookieContainer = new CookieContainer(),
            AllowAutoRedirect = true,
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli
        })
        .SetHandlerLifetime(Timeout.InfiniteTimeSpan);

        builder.Services.AddHttpClient(XShiftKeySource.HttpClientName, client =>
        {
            client.Timeout = TimeSpan.FromSeconds(20);
            client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Tomeshelf-SHiFT/1.0");
        })
        .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli
        });

        builder.Services.AddHttpClient(XAppOnlyTokenProvider.HttpClientName, client =>
        {
            client.Timeout = TimeSpan.FromSeconds(10);
            client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Tomeshelf-SHiFT/1.0");
        })
        .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli
        });
    }
}
