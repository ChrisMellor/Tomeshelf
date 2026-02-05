using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tomeshelf.Paissa.Application;
using Tomeshelf.Paissa.Infrastructure;
using Tomeshelf.ServiceDefaults;

namespace Tomeshelf.Paissa.Api;

/// <summary>
///     Entry point for the Paissa housing API.
/// </summary>
public static class Program
{
    /// <summary>
    ///     Configures and runs the web application.
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.AddServiceDefaults();

        if (builder.Environment.IsDevelopment())
        {
            builder.Services.AddHttpLogging(o =>
            {
                o.LoggingFields = HttpLoggingFields.RequestPath | HttpLoggingFields.RequestMethod | HttpLoggingFields.ResponseStatusCode | HttpLoggingFields.Duration;
                o.MediaTypeOptions.AddText("application/json");
            });
        }

        builder.Services
               .AddProblemDetails()
               .AddOpenApi()
               .AddControllers()
               .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });

        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        builder.Services.AddAuthorization();

        builder.Services.AddApplicationServices();
        builder.AddInfrastructureServices();

        var app = builder.Build();

        app.UseExceptionHandler();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();

            app.UseSwaggerUI(options =>
            {
                options.RoutePrefix = string.Empty;
                options.SwaggerEndpoint("/openapi/v1.json", "Tomeshelf Paissa API v1");
            });

            app.UseHttpLogging();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();
        app.MapControllers();

        app.MapExecutorDiscoveryEndpoint();
        app.MapDefaultEndpoints();

        await app.RunAsync();
    }
}