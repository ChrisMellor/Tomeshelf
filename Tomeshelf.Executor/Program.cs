using Quartz;
using Tomeshelf.Executor.Configuration;
using Tomeshelf.Executor.Jobs;
using Tomeshelf.Executor.Services;
using Tomeshelf.ServiceDefaults;

namespace Tomeshelf.Executor;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Configuration.AddJsonFile("executorSettings.json", optional: true, reloadOnChange: true);
        builder.Configuration.AddJsonFile($"executorSettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);
        builder.AddServiceDefaults();

        builder.Services.AddControllersWithViews();
        var executorSection = builder.Configuration.GetSection(ExecutorOptions.SectionName);
        builder.Services.Configure<ExecutorOptions>(executorSection);
        builder.Services.AddHttpClient(TriggerEndpointJob.HttpClientName);

        builder.Services.AddSingleton<IExecutorConfigurationStore, ExecutorConfigurationStore>();
        builder.Services.AddSingleton<IExecutorSchedulerOrchestrator, ExecutorSchedulerOrchestrator>();
        builder.Services.AddHostedService<ExecutorSchedulerHostedService>();

        builder.Services.AddQuartz();
        builder.Services.AddQuartzHostedService(options =>
        {
            options.WaitForJobsToComplete = true;
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.MapDefaultEndpoints();

        app.Run();
    }
}
