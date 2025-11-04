using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using Tomeshelf.Executor.Models;
using Tomeshelf.Executor.Options;
using Tomeshelf.Executor.Services;
using Tomeshelf.ServiceDefaults;

namespace Tomeshelf.Executor;

internal sealed class Program
{
    private const string HttpClientName = "executor";

    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.AddServiceDefaults();
        builder.Services.AddProblemDetails();
        builder.Services.AddControllersWithViews();
        builder.Services.AddAuthorization();

        builder.Services.AddOptions<ExecutorOptions>()
               .Bind(builder.Configuration.GetSection(ExecutorOptions.SectionName))
               .Validate(options => options is not null, "Executor options are required.")
               .ValidateOnStart();

        builder.Services.AddSingleton<EndpointCatalog>();
        builder.Services.AddScoped<EndpointExecutor>();
        builder.Services.AddHostedService<EndpointAutoDiscoveryService>();
        builder.Services.AddHostedService<ScheduledEndpointExecutor>();

        builder.Services.AddHttpClient(HttpClientName, client =>
                {
                    client.Timeout = Timeout.InfiniteTimeSpan;
                    client.DefaultRequestVersion = HttpVersion.Version20;
                    client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower;
                })
               .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
                {
                        AutomaticDecompression = DecompressionMethods.All,
                        AllowAutoRedirect = false
                });

        var app = builder.Build();

        ConfigurePipeline(app);
        MapRoutes(app);

        await app.RunAsync()
                 .ConfigureAwait(false);
    }

    private static void ConfigurePipeline(WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthorization();
    }

    private static void MapRoutes(WebApplication app)
    {
        app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
        app.MapControllers();
        app.MapDefaultEndpoints();
    }
}
