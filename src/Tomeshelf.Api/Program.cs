using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Tomeshelf.Api.Transformers;
using Tomeshelf.Application.Options;
using Tomeshelf.Infrastructure;
using Tomeshelf.Infrastructure.Persistence;
using Tomeshelf.ServiceDefaults;

namespace Tomeshelf.Api;

/// <summary>
/// API application entry point and configuration.
/// </summary>
public static class Program
{
    /// <summary>
    /// Application entry point for the API host.
    /// Configures services, runs migrations, and starts the web server.
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();

        builder.AddServiceDefaults();

        if (builder.Environment.IsDevelopment())
        {
            builder.Services.AddHttpLogging(o =>
            {
                o.LoggingFields = HttpLoggingFields.RequestPath | HttpLoggingFields.RequestMethod |
                                   HttpLoggingFields.RequestQuery | HttpLoggingFields.ResponseStatusCode |
                                   HttpLoggingFields.Duration | HttpLoggingFields.RequestHeaders | HttpLoggingFields.ResponseHeaders;
                o.RequestHeaders.Add("User-Agent");
                o.MediaTypeOptions.AddText("application/json");
                o.RequestBodyLogLimit = 0;
                o.ResponseBodyLogLimit = 0;
            });
        }

        builder.Services.AddProblemDetails()
            .AddOpenApi(options => { options.AddSchemaTransformer(new CitySchemaTransformer()); })
            .AddControllers()
            .AddJsonOptions(options => { options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()); });

        builder.Services.AddAuthorization();

        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.NumberHandling = JsonNumberHandling.AllowReadingFromString;
        });

        builder.Services.AddOptions<ComicConOptions>()
            .Bind(builder.Configuration)
            .ValidateDataAnnotations();

        builder.Services.AddInfrastructure();
        builder.Services.AddSingleton<Services.IGuestsCache, Services.GuestsCache>();
        builder.Services.AddHostedService<Hosted.ComicConUpdateBackgroundService>();

        builder.AddSqlServerDbContext<TomeshelfDbContext>("tomeshelfdb");

        var app = builder.Build();

        app.UseExceptionHandler();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();

            app.UseSwaggerUI(options =>
            {
                options.RoutePrefix = string.Empty;
                options.SwaggerEndpoint("/openapi/v1.json", "Tomeshelf API v1");
            });
        }

        app.UseHttpsRedirection();
        if (app.Environment.IsDevelopment())
        {
            app.UseHttpLogging();
        }
        app.UseAuthorization();
        app.MapControllers();

        app.MapDefaultEndpoints();

        if (args.Any(a => string.Equals(a, "--migrate", StringComparison.OrdinalIgnoreCase)))
        {
            using var scope = app.Services.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
                .CreateLogger("Migrations");
            logger.LogInformation("Starting database migrations...");
            var dbContext = scope.ServiceProvider.GetRequiredService<TomeshelfDbContext>();
            await dbContext.Database.MigrateAsync();
            logger.LogInformation("Database migrations completed successfully.");

            return;
        }

        if (app.Environment.IsDevelopment())
        {
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<TomeshelfDbContext>();
            await dbContext.Database.MigrateAsync();
        }

        await app.RunAsync();
    }
}
