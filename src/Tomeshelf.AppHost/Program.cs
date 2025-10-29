using System.Collections.Generic;
using Aspire.Hosting;
using Microsoft.Extensions.Configuration;
using Projects;
using Tomeshelf.AppHost.Records;

namespace Tomeshelf.AppHost;

/// <summary>
///     Aspire AppHost that defines and wires up application resources.
/// </summary>
internal class Program
{
    /// <summary>
    ///     Application entry point for the Aspire AppHost.
    ///     Defines resources and wiring for the distributed application.
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    public static void Main(string[] args)
    {
        var builder = DistributedApplication.CreateBuilder(args);

        builder.Configuration.AddUserSecrets<Program>(true);

        var isPublish = builder.ExecutionContext.IsPublishMode;

        if (!isPublish)
        {
            builder.AddDockerComposeEnvironment("metrics")
                   .WithDashboard(rb => rb.WithHostPort(18888));
        }

        var database = builder.AddSqlServer("sql")
                              .WithDataVolume()
                              .WithEnvironment("ACCEPT_EULA", "Y");

        var tomeshelfDb = database.AddDatabase("tomeshelfdb");
        var humbleBundleDb = database.AddDatabase("bundlesdb");
        var fitbitDb = database.AddDatabase("fitbitdb");

        var comicConApi = builder.AddProject<Tomeshelf_ComicConApi>("ComicConApi")
                                 .WithExternalHttpEndpoints()
                                 .WithHttpHealthCheck("/health")
                                 .WithReference(tomeshelfDb)
                                 .WaitFor(tomeshelfDb);

        var humbleBundleApi = builder.AddProject<Tomeshelf_HumbleBundle_Api>("HumbleBundleApi")
                                     .WithExternalHttpEndpoints()
                                     .WithHttpHealthCheck("/health")
                                     .WithReference(humbleBundleDb)
                                     .WaitFor(humbleBundleDb);

        var fitbitSettings = builder.Configuration.GetSection("Fitbit");

        var fitbitApi = builder.AddProject<Tomeshelf_Fitbit_Api>("FitbitApi")
                               .WithHttpsEndpoint(name: "fitbit-https", port: 7152)
                               .WithHttpEndpoint(name: "fitbit-http", port: 5152)
                               .WithHttpHealthCheck("/health")
                               .WithReference(fitbitDb)
                               .WaitFor(fitbitDb);

        fitbitApi = fitbitApi.WithEndpoint("fitbit-https", endpoint => endpoint.IsExternal = true)
                             .WithEndpoint("fitbit-http", endpoint => endpoint.IsExternal = true);

        var optionalEnv = new Dictionary<string, string>
        {
                ["Fitbit__ClientId"] = fitbitSettings["ClientId"],
                ["Fitbit__ClientSecret"] = fitbitSettings["ClientSecret"],
                ["Fitbit__AccessToken"] = fitbitSettings["AccessToken"],
                ["Fitbit__RefreshToken"] = fitbitSettings["RefreshToken"]
        };

        foreach (var entry in optionalEnv)
        {
            if (!string.IsNullOrWhiteSpace(entry.Value))
            {
                fitbitApi = fitbitApi.WithEnvironment(entry.Key, entry.Value!);
            }
        }

        fitbitApi = fitbitApi.WithEnvironment("Fitbit__ApiBase", fitbitSettings["ApiBase"] ?? "https://api.fitbit.com/")
                             .WithEnvironment("Fitbit__UserId", fitbitSettings["UserId"] ?? "-")
                             .WithEnvironment("Fitbit__Scope", fitbitSettings["Scope"] ?? "activity nutrition sleep weight profile settings")
                             .WithEnvironment("Fitbit__CallbackBaseUri", fitbitSettings["CallbackBaseUri"] ?? "https://localhost:7152")
                             .WithEnvironment("Fitbit__CallbackPath", fitbitSettings["CallbackPath"] ?? "/api/fitbit/auth/callback");

        builder.AddProject<Tomeshelf_Web>("web")
               .WithExternalHttpEndpoints()
               .WithHttpHealthCheck("/health")
               .WithReference(comicConApi)
               .WaitFor(comicConApi)
               .WithReference(humbleBundleApi)
               .WaitFor(humbleBundleApi)
               .WithReference(fitbitApi)
               .WaitFor(fitbitApi);

        var sites = builder.Configuration.GetSection("ComicCon")
                           .Get<List<ComicConSite>>() ?? [];

        for (var i = 0; i < sites.Count; i++)
        {
            comicConApi.WithEnvironment($"ComicCon__{i}__City", sites[i].City)
                       .WithEnvironment($"ComicCon__{i}__Key", sites[i]
                                                              .Key.ToString());
        }

        builder.Build()
               .Run();
    }
}