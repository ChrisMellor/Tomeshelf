using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;
using System;
using Tomeshelf.Executor.Configuration;
using Tomeshelf.Executor.Jobs;
using Tomeshelf.Executor.Services;
using Tomeshelf.ServiceDefaults;

namespace Tomeshelf.Executor;

public class Program
{
    public static WebApplication BuildApp(string[] args, Action<WebApplicationBuilder>? configureBuilder = null)
    {
        var builder = WebApplication.CreateBuilder(args);

        configureBuilder?.Invoke(builder);

        ExecutorSettingsPaths.EnsureSeedFiles(builder.Environment);
        builder.Configuration.AddJsonFile(ExecutorSettingsPaths.GetDefaultFilePath(builder.Environment), true, true);
        var environmentSettingsPath = ExecutorSettingsPaths.GetEnvironmentFilePath(builder.Environment);
        if (!string.IsNullOrWhiteSpace(environmentSettingsPath))
        {
            builder.Configuration.AddJsonFile(environmentSettingsPath, true, true);
        }

        builder.AddServiceDefaults();

        builder.Services.AddControllersWithViews();
        var executorSection = builder.Configuration.GetSection(ExecutorOptions.SectionName);
        builder.Services.Configure<ExecutorOptions>(executorSection);
        builder.Services.AddHttpClient(TriggerEndpointJob.HttpClientName);
        builder.Services.AddHttpClient(ApiEndpointDiscoveryService.HttpClientName);
        builder.Services.AddSingleton<IEndpointPingService, EndpointPingService>();

        builder.Services.AddSingleton<IExecutorConfigurationStore, ExecutorConfigurationStore>();
        builder.Services.AddSingleton<IExecutorSchedulerOrchestrator, ExecutorSchedulerOrchestrator>();
        builder.Services.AddSingleton<IApiEndpointDiscoveryService, ApiEndpointDiscoveryService>();
        builder.Services.AddHostedService<ExecutorSchedulerHostedService>();

        builder.Services.AddQuartz();
        builder.Services.AddQuartzHostedService(options =>
        {
            options.WaitForJobsToComplete = true;
        });

        var app = builder.Build();

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

        app.MapDefaultEndpoints();

        return app;
    }

    public static void Main(string[] args)
    {
        var app = BuildApp(args);
        app.Run();
    }
}