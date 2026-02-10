using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Docker.Resources.ServiceNodes;
using Aspire.Hosting.Yarp;
using Aspire.Hosting.Yarp.Transforms;
using Microsoft.Extensions.Configuration;
using Projects;
using System;
using System.Collections.Generic;
using System.IO;

namespace Tomeshelf.AppHost;

/// <summary>
///     Provides the entry point and configuration logic for the Aspire AppHost, orchestrating the setup and composition of
///     distributed application resources such as APIs, databases, and gateways.
/// </summary>
/// <remarks>
///     The Program class is responsible for initializing and wiring together all components required for the
///     distributed application, including configuring environment variables, Docker Compose resources, and service
///     dependencies. It exposes methods to build and configure the application, allowing for extensibility via optional
///     builder configuration delegates. This class is intended to be used as the main entry point for launching and
///     managing the distributed application's lifecycle.
/// </remarks>
public class Program
{
    private const string ContainerExecutorSettingsDirectory = "/home/app/.tomeshelf/executor";
    private const string ExecutorSettingsDirectoryVariable = "EXECUTOR_SETTINGS_DIR";
    private const string ExecutorSettingsVolumeName = "executor-settings";
    private const string SqlDataVolumeName = "tomeshelf-sql-data";

    /// <summary>
    ///     Creates and builds the distributed application configured by this AppHost.
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    /// <param name="configureBuilder">
    ///     Optional callback invoked after the builder is created (and user secrets are added) to allow callers to further
    ///     customize the builder before resources are registered.
    /// </param>
    /// <returns>The built <see cref="DistributedApplication" /> instance.</returns>
    public static DistributedApplication BuildApp(string[] args, Action<IDistributedApplicationBuilder>? configureBuilder = null)
    {
        var builder = CreateBuilder(args, configureBuilder);

        return builder.Build();
    }

    /// <summary>
    ///     Creates and configures the distributed application builder and registers all resources (databases, APIs, gateway,
    ///     and UI projects).
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    /// <param name="configureBuilder">
    ///     Optional callback invoked after the builder is created (and user secrets are added) to allow callers to further
    ///     customize the builder before resources are registered.
    /// </param>
    /// <returns>The configured <see cref="IDistributedApplicationBuilder" />.</returns>
    public static IDistributedApplicationBuilder CreateBuilder(string[] args, Action<IDistributedApplicationBuilder>? configureBuilder = null)
    {
        var options = new DistributedApplicationOptions
        {
            Args = args,
            AssemblyName = typeof(Program).Assembly.GetName()
                                          .Name
        };
        var builder = DistributedApplication.CreateBuilder(options);
        builder.Configuration.AddUserSecrets<Program>(true);
        configureBuilder?.Invoke(builder);

        var database = SetupDatabase(builder);

        var mcmApi = SetupMcmApi(builder, database);
        var humbleBundleApi = SetupHumbleBundleApi(builder, database);
        var fitbitApi = SetupFitbitApi(builder, database);
        var paissaApi = SetupPaissaApi(builder);
        var fileUploaderApi = SetupFileUploaderApi(builder);
        var shiftApi = SetupShiftApi(builder, database);
        var gateway = SetupGateway(builder, mcmApi, humbleBundleApi, fitbitApi, paissaApi, fileUploaderApi, shiftApi);

        _ = SetupExecutor(builder, mcmApi, humbleBundleApi, fitbitApi, paissaApi, fileUploaderApi, shiftApi);
        _ = SetupWeb(builder, gateway);

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

        return builder;
    }

    /// <summary>
    ///     Application entry point for the Aspire AppHost.
    ///     Defines resources and wiring for the distributed application.
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    public static void Main(string[] args)
    {
        var app = BuildApp(args);
        app.Run();
    }

    /// <summary>
    ///     Applies SHiFT Key scanner settings to the SHiFT API resource by mapping configuration values to environment
    ///     variables.
    /// </summary>
    /// <param name="api">The SHiFT API resource builder to apply settings to.</param>
    /// <param name="scanner">The <c>ShiftKeyScanner</c> configuration section.</param>
    private static void ApplyShiftScannerSettings(IResourceBuilder<ProjectResource> api, IConfigurationSection scanner)
    {
        ArgumentNullException.ThrowIfNull(api);
        ArgumentNullException.ThrowIfNull(scanner);

        ApplyValue(api, "ShiftKeyScanner__LookbackHours", scanner["LookbackHours"]);

        var xSection = scanner.GetSection("X");
        ApplyValue(api, "ShiftKeyScanner__X__Enabled", xSection["Enabled"]);
        ApplyValue(api, "ShiftKeyScanner__X__ApiBaseV2", xSection["ApiBaseV2"]);
        ApplyValue(api, "ShiftKeyScanner__X__OAuthTokenEndpoint", xSection["OAuthTokenEndpoint"]);
        ApplyValue(api, "ShiftKeyScanner__X__BearerToken", xSection["BearerToken"]);
        ApplyValue(api, "ShiftKeyScanner__X__ApiKey", xSection["ApiKey"]);
        ApplyValue(api, "ShiftKeyScanner__X__ApiSecret", xSection["ApiSecret"]);
        ApplyValue(api, "ShiftKeyScanner__X__TokenCacheMinutes", xSection["TokenCacheMinutes"]);
        ApplyValue(api, "ShiftKeyScanner__X__MaxPages", xSection["MaxPages"]);
        ApplyValue(api, "ShiftKeyScanner__X__MaxResultsPerPage", xSection["MaxResultsPerPage"]);
        ApplyValue(api, "ShiftKeyScanner__X__ExcludeReplies", xSection["ExcludeReplies"]);
        ApplyValue(api, "ShiftKeyScanner__X__ExcludeRetweets", xSection["ExcludeRetweets"]);

        var usernames = xSection.GetSection("Usernames")
                                .GetChildren();
        var index = 0;
        foreach (var username in usernames)
        {
            if (string.IsNullOrWhiteSpace(username.Value))
            {
                continue;
            }

            api.WithEnvironment($"ShiftKeyScanner__X__Usernames__{index}", username.Value);
            index++;
        }
    }

    /// <summary>
    ///     Applies a single environment value to a project resource if the value is present (non-empty).
    /// </summary>
    /// <param name="api">The project resource builder.</param>
    /// <param name="key">The environment variable key to set.</param>
    /// <param name="value">The environment variable value to set.</param>
    private static void ApplyValue(IResourceBuilder<ProjectResource> api, string key, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        api.WithEnvironment(key, value);
    }

    /// <summary>
    ///     Resolves the host directory used to store Executor settings when running outside containers.
    /// </summary>
    /// <remarks>
    ///     Prefers <see cref="Environment.SpecialFolder.LocalApplicationData" />, then <see cref="Environment.SpecialFolder.ApplicationData" />,
    ///     then <see cref="Environment.SpecialFolder.UserProfile" />, falling back to the current working directory.
    /// </remarks>
    /// <returns>The absolute path to the Executor settings directory on the host.</returns>
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

    /// <summary>
    ///     Adds the SQL Server container resource used by the application and configures persistence and Docker Compose
    ///     behavior.
    /// </summary>
    /// <param name="builder">The distributed application builder.</param>
    /// <returns>The SQL Server resource builder.</returns>
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

    /// <summary>
    ///     Registers the Executor UI/project and configures its settings directory mapping and dependencies on API services.
    /// </summary>
    /// <param name="builder">The distributed application builder.</param>
    /// <param name="apis">API project resources that the Executor depends on.</param>
    /// <returns>The Executor project resource builder.</returns>
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

    /// <summary>
    ///     Registers the File Uploader API project and forwards Google Drive configuration values as environment variables.
    /// </summary>
    /// <param name="builder">The distributed application builder.</param>
    /// <returns>The File Uploader API project resource builder.</returns>
    private static IResourceBuilder<ProjectResource> SetupFileUploaderApi(IDistributedApplicationBuilder builder)
    {
        var api = builder.AddProject<Tomeshelf_FileUploader_Api>("fileuploaderapi")
                         .WithHttpHealthCheck("/health")
                         .WithExternalHttpEndpoints()
                         .PublishAsDockerComposeService((resource, service) =>
                          {
                              service.Restart = "unless-stopped";
                          });

        var rootFolder = builder.Configuration.GetValue<string>("GoogleDrive:RootFolderPath");
        if (!string.IsNullOrWhiteSpace(rootFolder))
        {
            api.WithEnvironment("GoogleDrive__RootFolderPath", rootFolder);
        }

        var rootFolderId = builder.Configuration.GetValue<string>("GoogleDrive:RootFolderId");
        if (!string.IsNullOrWhiteSpace(rootFolderId))
        {
            api.WithEnvironment("GoogleDrive__RootFolderId", rootFolderId);
        }

        var clientId = builder.Configuration.GetValue<string>("GoogleDrive:ClientId");
        if (!string.IsNullOrWhiteSpace(clientId))
        {
            api.WithEnvironment("GoogleDrive__ClientId", clientId);
        }

        var clientSecret = builder.Configuration.GetValue<string>("GoogleDrive:ClientSecret");
        if (!string.IsNullOrWhiteSpace(clientSecret))
        {
            api.WithEnvironment("GoogleDrive__ClientSecret", clientSecret);
        }

        var userEmail = builder.Configuration.GetValue<string>("GoogleDrive:UserEmail");
        if (!string.IsNullOrWhiteSpace(userEmail))
        {
            api.WithEnvironment("GoogleDrive__UserEmail", userEmail);
        }

        var sharedDriveId = builder.Configuration.GetValue<string>("GoogleDrive:SharedDriveId");
        if (!string.IsNullOrWhiteSpace(sharedDriveId))
        {
            api.WithEnvironment("GoogleDrive__SharedDriveId", sharedDriveId);
        }

        return api;
    }

    /// <summary>
    ///     Registers the Fitbit API project, provisions its database, and maps Fitbit configuration values into environment
    ///     variables (including callback settings).
    /// </summary>
    /// <param name="builder">The distributed application builder.</param>
    /// <param name="database">The shared SQL Server resource.</param>
    /// <returns>The Fitbit API project resource builder.</returns>
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

    /// <summary>
    ///     Registers the YARP gateway and configures routes to backend APIs, exposing a stable host port for external
    ///     access.
    /// </summary>
    /// <param name="builder">The distributed application builder.</param>
    /// <param name="mcmApi">The MCM API resource.</param>
    /// <param name="humbleBundleApi">The Humble Bundle API resource.</param>
    /// <param name="fitbitApi">The Fitbit API resource.</param>
    /// <param name="paissaApi">The Paissa API resource.</param>
    /// <param name="fileUploaderApi">The File Uploader API resource.</param>
    /// <param name="shiftApi">The SHiFT API resource.</param>
    /// <returns>The YARP gateway resource builder.</returns>
    private static IResourceBuilder<YarpResource> SetupGateway(IDistributedApplicationBuilder builder, IResourceBuilder<ProjectResource> mcmApi, IResourceBuilder<ProjectResource> humbleBundleApi, IResourceBuilder<ProjectResource> fitbitApi, IResourceBuilder<ProjectResource> paissaApi, IResourceBuilder<ProjectResource> fileUploaderApi, IResourceBuilder<ProjectResource> shiftApi)
    {
        var gateway = builder.AddYarp("gateway")
                             .WithHostPort(5000)
                             .WithHttpHealthCheck("/health")
                             .PublishAsDockerComposeService((resource, service) =>
                              {
                                  service.Restart = "unless-stopped";
                                  // Publish the gateway to the host so external callers (and tooling) can reach APIs via /api/*.
                                  // Container-to-container calls still use the container port (5000).
                                  service.Ports ??= new List<string>();
                                  if (!service.Ports.Contains("5000:5000"))
                                  {
                                      service.Ports.Add("5000:5000");
                                  }
                              })
                             .WithConfiguration(yarp =>
                              {
                                  yarp.AddRoute("/health", mcmApi);
                                  yarp.AddRoute("/", mcmApi)
                                      .WithTransformPathSet("/health");

                                  yarp.AddRoute("/api/mcm/{**catch-all}", mcmApi)
                                      .WithTransformPathRemovePrefix("/api/mcm");

                                  yarp.AddRoute("/api/humblebundle/{**catch-all}", humbleBundleApi)
                                      .WithTransformPathRemovePrefix("/api/humblebundle");

                                  yarp.AddRoute("/api/fileuploader/{**catch-all}", fileUploaderApi)
                                      .WithTransformPathRemovePrefix("/api/fileuploader");

                                  yarp.AddRoute("/api/shift/{**catch-all}", shiftApi)
                                      .WithTransformPathRemovePrefix("/api/shift");

                                  yarp.AddRoute("/api/fitbit/{**catch-all}", fitbitApi);

                                  yarp.AddRoute("/api/paissa/{**catch-all}", paissaApi)
                                      .WithTransformPathRemovePrefix("/api/paissa")
                                      .WithTransformPathPrefix("/paissa");
                              });

        gateway.WithReference(mcmApi)
               .WaitFor(mcmApi);

        gateway.WithReference(humbleBundleApi)
               .WaitFor(humbleBundleApi);

        gateway.WithReference(fitbitApi)
               .WaitFor(fitbitApi);

        gateway.WithReference(paissaApi)
               .WaitFor(paissaApi);

        gateway.WithReference(fileUploaderApi)
               .WaitFor(fileUploaderApi);

        gateway.WithReference(shiftApi)
               .WaitFor(shiftApi);

        return gateway;
    }

    /// <summary>
    ///     Registers the Humble Bundle API project and provisions its database.
    /// </summary>
    /// <param name="builder">The distributed application builder.</param>
    /// <param name="database">The shared SQL Server resource.</param>
    /// <returns>The Humble Bundle API project resource builder.</returns>
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

    /// <summary>
    ///     Registers the MCM API project and provisions its database.
    /// </summary>
    /// <param name="builder">The distributed application builder.</param>
    /// <param name="database">The shared SQL Server resource.</param>
    /// <returns>The MCM API project resource builder.</returns>
    private static IResourceBuilder<ProjectResource> SetupMcmApi(IDistributedApplicationBuilder builder, IResourceBuilder<SqlServerServerResource> database)
    {
        var db = database.AddDatabase("mcmdb");

        var api = builder.AddProject<Tomeshelf_MCM_Api>("mcmapi")
                         .WithHttpHealthCheck("/health")
                         .WithReference(db)
                         .WaitFor(db)
                         .PublishAsDockerComposeService((resource, service) =>
                          {
                              service.Restart = "unless-stopped";
                          });

        return api;
    }

    /// <summary>
    ///     Registers the Paissa API project.
    /// </summary>
    /// <param name="builder">The distributed application builder.</param>
    /// <returns>The Paissa API project resource builder.</returns>
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

    /// <summary>
    ///     Registers the SHiFT API project, provisions its database, and applies scanner settings from configuration.
    /// </summary>
    /// <param name="builder">The distributed application builder.</param>
    /// <param name="database">The shared SQL Server resource.</param>
    /// <returns>The SHiFT API project resource builder.</returns>
    private static IResourceBuilder<ProjectResource> SetupShiftApi(IDistributedApplicationBuilder builder, IResourceBuilder<SqlServerServerResource> database)
    {
        var db = database.AddDatabase("shiftdb");

        var api = builder.AddProject<Tomeshelf_SHiFT_Api>("shiftapi")
                         .WithHttpHealthCheck("/health")
                         .WithReference(db)
                         .WaitFor(db)
                         .PublishAsDockerComposeService((resource, service) =>
                          {
                              service.Restart = "unless-stopped";
                          });

        var scanner = builder.Configuration.GetSection("ShiftKeyScanner");
        ApplyShiftScannerSettings(api, scanner);

        return api;
    }

    /// <summary>
    ///     Registers the MVC web frontend project, forwards Google Drive configuration values, and configures it to use the
    ///     YARP gateway for backend API access.
    /// </summary>
    /// <param name="builder">The distributed application builder.</param>
    /// <param name="gateway">The YARP gateway resource.</param>
    /// <returns>The Web project resource builder.</returns>
    private static IResourceBuilder<ProjectResource> SetupWeb(IDistributedApplicationBuilder builder, IResourceBuilder<YarpResource> gateway)
    {
        var web = builder.AddProject<Tomeshelf_Web>("web")
                         .WithHttpHealthCheck("/health")
                         .WithExternalHttpEndpoints()
                         .PublishAsDockerComposeService((resource, service) =>
                          {
                              service.Restart = "unless-stopped";
                          });

        var drive = builder.Configuration.GetSection("GoogleDrive");
        var clientId = drive.GetValue<string>("ClientId");
        if (!string.IsNullOrWhiteSpace(clientId))
        {
            web.WithEnvironment("GoogleDrive__ClientId", clientId);
        }

        var rootFolderId = drive.GetValue<string>("RootFolderId");
        if (!string.IsNullOrWhiteSpace(rootFolderId))
        {
            web.WithEnvironment("GoogleDrive__RootFolderId", rootFolderId);
        }

        var clientSecret = drive.GetValue<string>("ClientSecret");
        if (!string.IsNullOrWhiteSpace(clientSecret))
        {
            web.WithEnvironment("GoogleDrive__ClientSecret", clientSecret);
        }

        var userEmail = drive.GetValue<string>("UserEmail");
        if (!string.IsNullOrWhiteSpace(userEmail))
        {
            web.WithEnvironment("GoogleDrive__UserEmail", userEmail);
        }

        web.WithReference(gateway)
           .WaitFor(gateway);

        return web;
    }
}
