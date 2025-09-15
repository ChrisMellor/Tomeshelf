using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using Tomeshelf.AppHost.Records;

namespace Tomeshelf.AppHost;

/// <summary>
/// Aspire AppHost that defines and wires up application resources.
/// </summary>
internal class Program
{
    /// <summary>
    /// Application entry point for the Aspire AppHost.
    /// Defines resources and wiring for the distributed application.
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    public static void Main(string[] args)
    {
        var builder = DistributedApplication.CreateBuilder(args);

        builder.Configuration.AddUserSecrets<Program>(optional: true);

        var isPublish = builder.ExecutionContext.IsPublishMode;

        if (!isPublish)
        {
            builder.AddDockerComposeEnvironment("metrics")
                .WithDashboard(resourceBuilder => resourceBuilder.WithHostPort(18888));
        }

        var sql = builder.AddSqlServer("sql")
            .WithLifetime(ContainerLifetime.Persistent)
            .WithDataVolume();

        var database = sql.AddDatabase("appdb");

        var api = builder.AddProject<Projects.Tomeshelf_Api>("api")
            .WithHttpEndpoint(name: "api-http", targetPort: 5280)
            .WithExternalHttpEndpoints()
            .WithHttpHealthCheck("/health")
            .WithReference(database)
            .WaitFor(database);

        builder.AddProject<Projects.Tomeshelf_Web>("web")
            .WithExternalHttpEndpoints()
            .WithHttpHealthCheck("/health")
            .WithReference(api)
            .WaitFor(api);

        var sites = builder.Configuration
            .GetSection("ComicCon")
            .Get<List<ComicConSite>>() ?? [];

        for (var i = 0; i < sites.Count; i++)
        {
            api.WithEnvironment($"ComicCon__{i}__City", sites[i].City)
                .WithEnvironment($"ComicCon__{i}__Key", sites[i].Key.ToString());
        }

        builder.Build().Run();
    }
}
