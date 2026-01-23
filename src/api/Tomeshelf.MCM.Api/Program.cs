using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Tomeshelf.Infrastructure.Shared.Persistence;
using Tomeshelf.Mcm.Api.Clients;
using Tomeshelf.Mcm.Api.Mappers;
using Tomeshelf.Mcm.Api.Repositories;
using Tomeshelf.Mcm.Api.Services;
using Tomeshelf.Mcm.Api.Transformers;
using Tomeshelf.ServiceDefaults;

namespace Tomeshelf.Mcm.Api;

/// <summary>
///     Provides the entry point for the application and configures the web host and services.
/// </summary>
/// <remarks>
///     The Program class contains the Main method, which initializes and runs the web application. It is
///     responsible for setting up dependency injection, configuring middleware, and starting the HTTP server. This class
///     is
///     typically not instantiated directly.
/// </remarks>
public class Program
{
    /// <summary>
    ///     Configures and runs the Tomeshelf.Mcm.Api web application.
    /// </summary>
    /// <remarks>
    ///     This method sets up application services, configures controllers, OpenAPI/Swagger, database
    ///     context, and HTTP clients, applies any pending database migrations, and starts the web server. It is the entry
    ///     point for the application and is typically invoked by the runtime.
    /// </remarks>
    /// <param name="args">An array of command-line arguments to configure the application at startup.</param>
    /// <returns>A task that represents the asynchronous operation of running the web application.</returns>
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.AddServiceDefaults();

        builder.Services
               .AddControllers()
               .AddJsonOptions(o =>
                {
                    o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });

        builder.Services.AddOpenApi(options =>
        {
            options.AddSchemaTransformer<EnumAsStringSchemaTransformer>();
        });

        builder.AddSqlServerDbContext<TomeshelfMcmDbContext>("mcmdb");

        builder.Services.AddSingleton<IGuestMapper, GuestMapper>();
        builder.Services.AddScoped<IGuestsRepository, GuestsRepository>();
        builder.Services.AddScoped<IGuestsService, GuestsService>();
        builder.Services.AddHttpClient<IMcmGuestsClient, McmGuestsClient>(client =>
        {
            client.BaseAddress = new Uri("https://conventions.leapevent.tech/");
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Tomeshelf-McmApi/1.0");
        });

        builder.Services.AddScoped<IEventRepository, EventRepository>();
        builder.Services.AddScoped<IEventService, EventService>();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.UseSwaggerUI(options =>
            {
                options.RoutePrefix = string.Empty;
                options.SwaggerEndpoint("/openapi/v1.json", "Tomeshelf.Mcm.Api v1");
            });
        }

        app.UseHttpsRedirection();
        app.MapControllers();

        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TomeshelfMcmDbContext>();
            await db.Database.MigrateAsync();
        }

        app.MapExecutorDiscoveryEndpoint();
        app.MapDefaultEndpoints();

        await app.RunAsync();
    }
}