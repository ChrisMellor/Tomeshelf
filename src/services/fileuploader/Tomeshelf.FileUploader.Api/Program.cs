using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Tomeshelf.FileUploader.Application;
using Tomeshelf.FileUploader.Infrastructure;
using Tomeshelf.ServiceDefaults;

namespace Tomeshelf.FileUploader.Api;

public class Program
{
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
               .AddControllers();

        builder.Services.AddAuthorization();
        builder.Services.Configure<GoogleDriveOptions>(builder.Configuration.GetSection("GoogleDrive"));
        builder.Services.Configure<FormOptions>(options =>
        {
            options.MultipartBodyLengthLimit = 1_073_741_824; // ~1GB
        });

        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.NumberHandling = JsonNumberHandling.AllowReadingFromString;
        });

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
                options.SwaggerEndpoint("/openapi/v1.json", "File Uploader API v1");
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