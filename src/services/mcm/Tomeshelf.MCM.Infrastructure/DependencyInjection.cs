using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using Tomeshelf.MCM.Application.Abstractions.Clients;
using Tomeshelf.MCM.Application.Abstractions.Persistence;
using Tomeshelf.MCM.Infrastructure.Clients;
using Tomeshelf.MCM.Infrastructure.Repositories;

namespace Tomeshelf.MCM.Infrastructure;

public static class DependencyInjection
{
    private const string ConnectionName = "mcmdb";

    public static void AddInfrastructureServices(this IHostApplicationBuilder builder)
    {
        var connectionString = builder.Configuration[$"ConnectionStrings:{ConnectionName}"];
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException($"Connection string '{ConnectionName}' is missing.");
        }

        builder.Services.AddDbContext<TomeshelfMcmDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
        });

        builder.Services.AddScoped<IEventRepository, EventRepository>();
        builder.Services.AddScoped<IGuestsRepository, GuestsRepository>();

        builder.Services.AddHttpClient<IMcmGuestsClient, McmGuestsClient>(client =>
        {
            client.BaseAddress = new Uri("https://conventions.leapevent.tech/");
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Tomeshelf-McmApi/1.0");
        });
    }
}
