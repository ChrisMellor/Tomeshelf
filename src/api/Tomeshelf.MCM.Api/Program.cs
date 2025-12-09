using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json.Serialization;
using Tomeshelf.Infrastructure.Persistence;
using Tomeshelf.MCM.Api.Clients;
using Tomeshelf.MCM.Api.Repositories;
using Tomeshelf.MCM.Api.Services;
using Tomeshelf.MCM.Api.Transformers;

namespace Tomeshelf.MCM.Api;

/// <summary>
///     Provides the entry point for the application and configures the web host, services, and HTTP request pipeline.
/// </summary>
/// <remarks>
///     This class is responsible for initializing and running the web application. It sets up dependency
///     injection, configures middleware, and starts the HTTP server. The application remains running until it is shut
///     down.
///     Typically, this class is not used directly by other components; it is invoked automatically when the application
///     starts.
/// </remarks>
public class Program
{
    /// <summary>
    ///     Configures and runs the web application using the specified command-line arguments.
    /// </summary>
    /// <remarks>
    ///     This method serves as the entry point for the application. It sets up services, configures
    ///     middleware, and starts the HTTP server. The method does not return until the application shuts down.
    /// </remarks>
    /// <param name="args">An array of command-line arguments to configure the application's behavior.</param>
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers()
               .AddJsonOptions(o =>
                {
                    o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });

        builder.Services.AddOpenApi(options =>
        {
            options.AddSchemaTransformer<EnumAsStringSchemaTransformer>();
        });

        builder.AddSqlServerDbContext<TomeshelfMcmDbContext>("mcmdb");

        builder.Services.AddScoped<IGuestsService, GuestsService>();
        builder.Services.AddScoped<IMcmGuestsClient, McmGuestsClient>();
        builder.Services.AddScoped<IGuestsRepository, GuestsRepository>();
        builder.Services.AddScoped<IEventConfigService, EventConfigService>();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.UseSwaggerUI(options =>
            {
                options.RoutePrefix = string.Empty;
                options.SwaggerEndpoint("/openapi/v1.json", "Tomeshelf.MCM.Api v1");
            });
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}