using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
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

        builder.AddSqlServerDbContext<TomeshelfMcmDbContext>("shiftdb");

        builder.Services
               .AddDataProtection()
               .PersistKeysToDbContext<TomeshelfShiftDbContext>();

        builder.Services.AddScoped<IShiftWebSession, ShiftWebSession>();
        builder.Services.AddScoped<IGearboxService, GearboxService>();
        builder.Services.AddScoped<IShiftSettingsStore, ShiftSettingsStore>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        await app.RunAsync();
    }
}