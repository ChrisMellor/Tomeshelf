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
    private const string ApiBasePath = "/api/executor";

    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.AddServiceDefaults();
        builder.Services.AddProblemDetails();

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
        app.UseDefaultFiles();
        app.UseStaticFiles();
        app.UseRouting();
    }

    private static void MapRoutes(WebApplication app)
    {
        app.MapGet("/error", (HttpContext context) =>
        {
            var error = context.Features.Get<IExceptionHandlerFeature>()
                              ?.Error;

            return Results.Problem(error?.Message ?? "An unexpected error occurred.");
        });

        app.MapGet($"{ApiBasePath}/endpoints", (EndpointCatalog catalog) =>
        {
            var summaries = catalog.GetSummaries();

            return Results.Ok(summaries);
        });

        app.MapGet($"{ApiBasePath}/endpoints/{{id}}/next", (string id, EndpointCatalog catalog) =>
        {
            if (!catalog.TryGetDescriptor(id, out var descriptor))
            {
                return Results.NotFound();
            }

            if (descriptor.CronExpression is null)
            {
                return Results.NoContent();
            }

            var occurrences = new List<DateTimeOffset>();
            var reference = DateTimeOffset.UtcNow;
            var timeZoneId = descriptor.TimeZone?.Id ?? descriptor.CronExpression.TimeZone?.Id ?? TimeZoneInfo.Utc.Id;

            for (var i = 0; i < 5; i++)
            {
                var next = descriptor.CronExpression.GetNextValidTimeAfter(reference);
                if (next is null)
                {
                    break;
                }

                occurrences.Add(next.Value);
                reference = next.Value;
            }

            return Results.Ok(new
            {
                    timeZone = timeZoneId,
                    occurrences
            });
        });

        app.MapPost($"{ApiBasePath}/endpoints/{{id}}/execute", async (string id, EndpointExecutionRequest request, EndpointExecutor executor, CancellationToken cancellationToken) =>
        {
            try
            {
                var result = await executor.ExecuteAsync(id, request, cancellationToken)
                                           .ConfigureAwait(false);

                return Results.Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
        });

        app.MapDefaultEndpoints();
        app.MapFallbackToFile("index.html");
    }
}