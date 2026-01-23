using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tomeshelf.Application.Shared.Abstractions.SHiFT;
using Tomeshelf.Application.Shared.Services.SHiFT;
using Tomeshelf.Infrastructure.Shared.Persistence;
using Tomeshelf.Infrastructure.Shared.SHiFT;
using Tomeshelf.ServiceDefaults;

namespace Tomeshelf.SHiFT.Api;

/// <summary>
///     Configures and runs the Tomeshelf Shift web application.
/// </summary>
/// <remarks>
///     This entry point sets up application services, configures middleware, applies database migrations,
///     and starts the web server. It enables controllers, OpenAPI/Swagger documentation, SQL Server database context, data
///     protection with persisted keys, and application-specific services. In development environments, it also configures
///     the Swagger UI for API exploration.
/// </remarks>
public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.AddServiceDefaults();

        builder.Services.AddControllers();
        builder.Services.AddOpenApi();

        builder.AddSqlServerDbContext<TomeshelfShiftDbContext>("shiftdb");

        builder.Services
               .AddDataProtection()
               .PersistKeysToDbContext<TomeshelfShiftDbContext>();

        builder.Services.AddSingleton<IShiftWebSessionFactory, ShiftWebSessionFactory>();
        builder.Services.AddScoped<IGearboxService, GearboxService>();
        builder.Services.AddScoped<IShiftSettingsStore, ShiftSettingsStore>();

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