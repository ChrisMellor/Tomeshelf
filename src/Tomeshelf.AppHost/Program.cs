using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Docker.Resources.ServiceNodes;
using Microsoft.Extensions.Configuration;
using Projects;
using System;
using System.Collections.Generic;
using System.IO;
using Tomeshelf.AppHost.Records;

namespace Tomeshelf.AppHost;

/// <summary>
///     Aspire AppHost that defines and wires up application resources.
/// </summary>
internal class Program
{
    private const string ContainerExecutorSettingsDirectory = "/home/app/.tomeshelf/executor";
    private const string ExecutorSettingsDirectoryVariable = "EXECUTOR_SETTINGS_DIR";
    private const string ExecutorSettingsVolumeName = "executor-settings";
    private const string SqlDataVolumeName = "tomeshelf-sql-data";

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

        builder.AddDockerComposeEnvironment("tomeshelf")
               .ConfigureComposeFile(compose =>
                {
                    compose.AddVolume(new Volume
                    {
                        Name = ExecutorSettingsVolumeName,
                        Driver = "local"
                    });
                })
               .WithDashboard(rb => rb.WithHostPort(18888));

        builder.Build()
               .Run();
    }

    private static IResourceBuilder<SqlServerServerResource> SetupDatabase(IDistributedApplicationBuilder builder)
    {
        var database = builder.AddSqlServer("sql")
                              .WithDataVolume(SqlDataVolumeName)
                              .WithEnvironment("ACCEPT_EULA", "Y")
                              .PublishAsDockerComposeService((resource, service) =>
                               {
                                   service.Restart = "unless-stopped";
                               });

        return database;
    }

    private static IResourceBuilder<ProjectResource> SetupComicConApi(IDistributedApplicationBuilder builder, IResourceBuilder<SqlServerServerResource> database)
    {
        var db = database.AddDatabase("mcmdb");

        var api = builder.AddProject<Tomeshelf_ComicCon_Api>("comicconapi")
                         .WithHttpHealthCheck("/health")
                         .WithReference(db)
                         .WaitFor(db)
                         .PublishAsDockerComposeService((resource, service) =>
                          {
                              service.Restart = "unless-stopped";
                          });

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

    private static IResourceBuilder<ProjectResource> SetupHumbleBundleApi(IDistributedApplicationBuilder builder, IResourceBuilder<SqlServerServerResource> database)
    {
        var db = database.AddDatabase("humblebundledb");

        var api = builder.AddProject<Tomeshelf_HumbleBundle_Api>("humblebundleapi")
                         .WithHttpHealthCheck("/health")
                         .WithReference(db)
                         .WaitFor(db)
                         .PublishAsDockerComposeService((resource, service) =>
                          {
                              service.Restart = "unless-stopped";
                          });

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
                         .WaitFor(db)
                         .PublishAsDockerComposeService((resource, service) =>
                          {
                              service.Restart = "unless-stopped";
                          });

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
                         .WithHttpHealthCheck("/health")
                         .PublishAsDockerComposeService((resource, service) =>
                          {
                              service.Restart = "unless-stopped";
                          });

        return api;
    }

    private static IResourceBuilder<ProjectResource> SetupWeb(IDistributedApplicationBuilder builder, params IResourceBuilder<ProjectResource>[] apis)
    {
        var web = builder.AddProject<Tomeshelf_Web>("web")
                         .WithHttpHealthCheck("/health")
                         .WithExternalHttpEndpoints()
                         .PublishAsDockerComposeService((resource, service) =>
                          {
                              service.Restart = "unless-stopped";
                          });

        foreach (var api in apis)
        {
            web.WithReference(api)
               .WaitFor(api);
        }

        return web;
    }

    private static IResourceBuilder<ProjectResource> SetupExecutor(IDistributedApplicationBuilder builder, params IResourceBuilder<ProjectResource>[] apis)
    {
        var hostExecutorSettingsDirectory = ResolveHostExecutorSettingsDirectory();
        var volume = new Volume
        {
            Name = ExecutorSettingsVolumeName,
            Type = "volume",
            Source = ExecutorSettingsVolumeName,
            Target = ContainerExecutorSettingsDirectory,
            ReadOnly = false
        };

        var executor = builder.AddProject<Tomeshelf_Executor>("executor")
                              .WithHttpHealthCheck("/health")
                              .WithExternalHttpEndpoints()
                              .WithEnvironment(ExecutorSettingsDirectoryVariable, hostExecutorSettingsDirectory)
                              .PublishAsDockerComposeService((resource, service) =>
                               {
                                   service.Restart = "unless-stopped";
                                   service.User = "root";
                                   service.Environment ??= new Dictionary<string, string?>();
                                   service.Environment[ExecutorSettingsDirectoryVariable] = ContainerExecutorSettingsDirectory;
                                   service.AddVolume(volume);
                               });

        foreach (var api in apis)
        {
            executor.WithReference(api)
                    .WaitFor(api);
        }

        return executor;
    }

    private static string ResolveHostExecutorSettingsDirectory()
    {
        var basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        if (string.IsNullOrWhiteSpace(basePath))
        {
            basePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        }

        if (string.IsNullOrWhiteSpace(basePath))
        {
            basePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        }

        basePath = string.IsNullOrWhiteSpace(basePath)
                ? Directory.GetCurrentDirectory()
                : basePath;

        return Path.Combine(basePath, "Tomeshelf", "executor");
    }
}