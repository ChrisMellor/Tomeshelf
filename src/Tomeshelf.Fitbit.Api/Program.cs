using System.Text.Json.Serialization;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.EntityFrameworkCore;
using Tomeshelf.Application.Options;
using Tomeshelf.Infrastructure;
using Tomeshelf.Infrastructure.Persistence;
using Tomeshelf.ServiceDefaults;

namespace Tomeshelf.Fitbit.Api;

public class Program
{
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
                o.LoggingFields = HttpLoggingFields.RequestPath | HttpLoggingFields.RequestMethod | HttpLoggingFields.ResponseStatusCode | HttpLoggingFields.Duration | HttpLoggingFields.RequestHeaders | HttpLoggingFields.ResponseHeaders;
                o.RequestHeaders.Add("User-Agent");
                o.MediaTypeOptions.AddText("application/json");
            });
        }

        builder.Services.AddProblemDetails()
               .AddOpenApi()
               .AddControllers()
               .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });

        builder.Services.AddAuthorization();

        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        });

        builder.Services.AddOptions<FitbitOptions>()
               .Bind(builder.Configuration.GetSection("Fitbit"))
               .ValidateDataAnnotations();

        builder.Services.AddFitnessInfrastructure();
        builder.AddSqlServerDbContext<TomeshelfFitbitDbContext>("fitbitDb");

        var app = builder.Build();

        if (args.Any(a => string.Equals(a, "--migrate", StringComparison.OrdinalIgnoreCase)))
        {
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<TomeshelfFitbitDbContext>();
            await dbContext.Database.MigrateAsync();

            return;
        }

        if (app.Environment.IsDevelopment())
        {
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<TomeshelfFitbitDbContext>();
            await dbContext.Database.MigrateAsync();
        }

        app.UseExceptionHandler();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.UseSwaggerUI(options =>
            {
                options.RoutePrefix = string.Empty;
                options.SwaggerEndpoint("/openapi/v1.json", "Tomeshelf Fitbit API v1");
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

        await app.RunAsync();
    }
}