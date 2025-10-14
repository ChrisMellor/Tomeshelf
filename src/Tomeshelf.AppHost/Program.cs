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

        builder.AddProject<Tomeshelf_Web>("web")
               .WithExternalHttpEndpoints()
               .WithHttpHealthCheck("/health")
               .WithReference(comicConApi)
               .WaitFor(comicConApi)
               .WithReference(humbleBundleApi)
               .WaitFor(humbleBundleApi);

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