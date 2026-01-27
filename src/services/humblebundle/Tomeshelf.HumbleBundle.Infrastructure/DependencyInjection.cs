using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using Tomeshelf.HumbleBundle.Application.Abstractions.External;
using Tomeshelf.HumbleBundle.Application.Abstractions.Persistence;
using Tomeshelf.HumbleBundle.Infrastructure.Bundles;

namespace Tomeshelf.HumbleBundle.Infrastructure;

public static class DependencyInjection
{
    private const string ConnectionName = "humblebundledb";

    public static void AddInfrastructureServices(this IHostApplicationBuilder builder)
    {
        var connectionString = builder.Configuration[$"ConnectionStrings:{ConnectionName}"];
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException($"Connection string '{ConnectionName}' is missing.");
        }

        builder.Services.AddDbContext<TomeshelfBundlesDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
        });

        builder.Services.AddScoped<IHumbleBundleScraper, HumbleBundleScraper>();
        builder.Services.AddHttpClient(HumbleBundleScraper.HttpClientName, client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Tomeshelf-HumbleBundle/1.0");
        });

        builder.Services.AddScoped<IBundleQueries, BundleQueries>();
        builder.Services.AddScoped<IBundleIngestService, BundleIngestService>();
    }
}
