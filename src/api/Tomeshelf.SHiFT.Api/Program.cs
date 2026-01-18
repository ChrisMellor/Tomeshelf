using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using Tomeshelf.Application.SHiFT;
using Tomeshelf.Infrastructure.Persistence;
using Tomeshelf.Infrastructure.SHiFT;
using Tomeshelf.ServiceDefaults;

namespace Tomeshelf.SHiFT.Api;

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

        builder.Services.AddScoped<IShiftWebSession, ShiftWebSession>();
        builder.Services.AddScoped<IGearboxService, GearboxService>();
        builder.Services.AddScoped<IShiftSettingsStore, ShiftSettingsStore>();

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
            var db = scope.ServiceProvider.GetRequiredService<TomeshelfShiftDbContext>();
            await db.Database.MigrateAsync();
        }

        app.MapExecutorDiscoveryEndpoint();
        app.MapDefaultEndpoints();

        await app.RunAsync();
    }
}