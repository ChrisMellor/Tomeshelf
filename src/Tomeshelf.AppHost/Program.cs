using System.Collections.Generic;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
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

        var database = SetupDatabase(builder);

        var comicConApi = SetupComicConApi(builder, database);
        var humbleBundleApi = SetupHumbleBundleApi(builder, database);
        var fitbitApi = SetupFitbitApi(builder, database);
        var paissaApi = SetupPaissaApi(builder);

        _ = SetupExecutor(builder, comicConApi, humbleBundleApi, fitbitApi, paissaApi);
        _ = SetupWeb(builder, comicConApi, humbleBundleApi, fitbitApi, paissaApi);

        builder.AddDockerComposeEnvironment("compose")
               .WithDashboard(rb => rb.WithHostPort(18888));

        builder.Build()
               .Run();
    }

    private static IResourceBuilder<SqlServerServerResource> SetupDatabase(IDistributedApplicationBuilder builder)
    {
        var database = builder.AddSqlServer("sql")
                              .WithDataVolume()
                              .WithEnvironment("ACCEPT_EULA", "Y");

        return database;
    }

    private static IResourceBuilder<ProjectResource> SetupComicConApi(IDistributedApplicationBuilder builder, IResourceBuilder<SqlServerServerResource> database)
    {
        var db = database.AddDatabase("mcmdb");

        var api = builder.AddProject<Tomeshelf_ComicConApi>("comicconapi")
                         .WithHttpHealthCheck("/health")
                         .WithReference(db)
                         .WaitFor(db);

        var sites = builder.Configuration.GetSection("ComicCon")
                           .Get<List<ComicConSite>>() ?? [];

        for (var i = 0; i < sites.Count; i++)
        {
            api.WithEnvironment($"ComicCon__{i}__City", sites[i].City)
               .WithEnvironment($"ComicCon__{i}__Key", sites[i]
                                                      .Key.ToString());
        }

        return api;
    }

    private static IResourceBuilder<ProjectResource> SetupExecutor(IDistributedApplicationBuilder builder, IResourceBuilder<ProjectResource> comicConApi, IResourceBuilder<ProjectResource> humbleBundleApi, IResourceBuilder<ProjectResource> fitbitApi, IResourceBuilder<ProjectResource> paissaApi)
    {
        var Executor = builder.AddProject<Tomeshelf_Executor>("executor")
                              .WithHttpHealthCheck("/health")
                              .WithExternalHttpEndpoints()
                              .WithReference(comicConApi)
                              .WaitFor(comicConApi)
                              .WithReference(humbleBundleApi)
                              .WaitFor(humbleBundleApi)
                              .WithReference(fitbitApi)
                              .WaitFor(fitbitApi)
                              .WithReference(paissaApi)
                              .WaitFor(paissaApi);

        return Executor;
    }

    private static IResourceBuilder<ProjectResource> SetupHumbleBundleApi(IDistributedApplicationBuilder builder, IResourceBuilder<SqlServerServerResource> database)
    {
        var db = database.AddDatabase("humblebundledb");

        var api = builder.AddProject<Tomeshelf_HumbleBundle_Api>("humblebundleapi")
                         .WithHttpHealthCheck("/health")
                         .WithReference(db)
                         .WaitFor(db);

        return api;
    }

    private static IResourceBuilder<ProjectResource> SetupFitbitApi(IDistributedApplicationBuilder builder, IResourceBuilder<SqlServerServerResource> database)
    {
        var settings = builder.Configuration.GetSection("fitbit");

        var db = database.AddDatabase("fitbitdb");

        var api = builder.AddProject<Tomeshelf_Fitbit_Api>("fitbitapi")
                         .WithExternalHttpEndpoints()
                         .WithHttpHealthCheck("/health")
                         .WithReference(db)
                         .WaitFor(db);

        api.WithEnvironment("Fitbit__ApiBase", settings["ApiBase"] ?? "https://api.fitbit.com/")
           .WithEnvironment("Fitbit__UserId", settings["UserId"] ?? "-")
           .WithEnvironment("Fitbit__Scope", settings["Scope"] ?? "activity nutrition sleep weight profile settings")
           .WithEnvironment("Fitbit__CallbackBaseUri", api.GetEndpoint("https"))
           .WithEnvironment("Fitbit__CallbackPath", settings["CallbackPath"] ?? "/api/fitbit/auth/callback")
           .WithEnvironment("Fitbit__ClientId", settings["ClientId"])
           .WithEnvironment("Fitbit__ClientSecret", settings["ClientSecret"]);

        return api;
    }

    private static IResourceBuilder<ProjectResource> SetupPaissaApi(IDistributedApplicationBuilder builder)
    {
        var api = builder.AddProject<Tomeshelf_Paissa_Api>("paissaapi")
                         .WithHttpHealthCheck("/health");

        return api;
    }

    private static IResourceBuilder<ProjectResource> SetupWeb(IDistributedApplicationBuilder builder, IResourceBuilder<ProjectResource> comicConApi, IResourceBuilder<ProjectResource> humbleBundleApi, IResourceBuilder<ProjectResource> fitbitApi, IResourceBuilder<ProjectResource> paissaApi)
    {
        var web = builder.AddProject<Tomeshelf_Web>("web")
                         .WithHttpHealthCheck("/health")
                         .WithReference(comicConApi)
                         .WaitFor(comicConApi)
                         .WithReference(humbleBundleApi)
                         .WaitFor(humbleBundleApi)
                         .WithReference(fitbitApi)
                         .WaitFor(fitbitApi)
                         .WithReference(paissaApi)
                         .WaitFor(paissaApi)
                         .WithExternalHttpEndpoints();

        return web;
    }
}