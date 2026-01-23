using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Tomeshelf.Application.Shared.Options;
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

        builder.Services
               .AddProblemDetails()
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

        var config = builder.Configuration.GetSection("Fitbit");

        builder.Services
               .AddOptions<FitbitOptions>()
               .Bind(config)
               .ValidateDataAnnotations();

        builder.Services.AddFitnessInfrastructure();
        builder.AddSqlServerDbContext<TomeshelfFitbitDbContext>("fitbitDb");

        var app = builder.Build();

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
        app.MapExecutorDiscoveryEndpoint();
        app.MapDefaultEndpoints();

        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TomeshelfFitbitDbContext>();
            await db.Database.MigrateAsync();
        }

        await app.RunAsync();
    }
}