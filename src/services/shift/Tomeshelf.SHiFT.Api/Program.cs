using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using Tomeshelf.ServiceDefaults;
using Tomeshelf.SHiFT.Infrastructure;
using Tomeshelf.SHiFT.Infrastructure.Persistence;

namespace Tomeshelf.SHiFT.Api;

/// <summary>
///     Provides the entry point for the Tomeshelf.SHiFT.Api application, configuring and starting the web host with
///     required services, middleware, and endpoints.
/// </summary>
/// <remarks>
///     This class sets up essential application services, including controllers, database context, data
///     protection, and API documentation. It also applies database migrations at startup and configures middleware such as
///     HTTPS redirection and Swagger UI in development environments. The application is started asynchronously and is
///     intended to be run as the main process for the Tomeshelf.SHiFT.Api service.
/// </remarks>
public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.AddServiceDefaults();

        builder.Services.AddControllers();
        builder.Services.AddOpenApi();

        builder.AddInfrastructureServices();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.UseSwaggerUI(options =>
            {
                options.RoutePrefix = string.Empty;
                options.SwaggerEndpoint("/openapi/v1.json", "Tomeshelf.SHiFT.Api v1");
            });
        }

        app.UseHttpsRedirection();
        app.MapControllers();

        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TomeshelfShiftDbContext>();
            await db.Database.MigrateAsync();
        }

        app.MapExecutorDiscoveryEndpoint();
        app.MapDefaultEndpoints();

        await app.RunAsync();
    }
}