using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Text.Json.Serialization;
using Tomeshelf.Infrastructure.Persistence;
using Tomeshelf.Mcm.Api.Clients;
using Tomeshelf.Mcm.Api.Mappers;
using Tomeshelf.Mcm.Api.Repositories;
using Tomeshelf.Mcm.Api.Services;
using Tomeshelf.Mcm.Api.Transformers;

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
    ///     This method sets up services, middleware, and endpoints for the ASP.NET Core application,
    ///     including controllers, JSON serialization options, OpenAPI/Swagger documentation, database context, and HTTP
    ///     clients. It is the entry point of the application and is typically called by the runtime. No return value is
    ///     expected.
    /// </remarks>
    /// <param name="args">An array of command-line arguments to configure the application at startup.</param>
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

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

        app.Run();
    }
}