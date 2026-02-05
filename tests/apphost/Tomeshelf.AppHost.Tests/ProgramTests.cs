using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Tomeshelf.AppHost.Tests;

public class ProgramTests
{
    [Fact]
    public async Task CreateBuilder_adds_expected_resources()
    {
        await using var scope = new AppScope();
        var model = scope.App.Services.GetRequiredService<DistributedApplicationModel>();
        var names = model.Resources
                         .Select(resource => resource.Name)
                         .ToArray();

        names.Should()
             .Contain(new[] { "sql", "mcmdb", "humblebundledb", "fitbitdb", "shiftdb", "mcmapi", "humblebundleapi", "fitbitapi", "paissaapi", "fileuploaderapi", "shiftapi", "executor", "web" });
    }

    [Fact]
    public async Task Executor_sets_settings_directory()
    {
        await using var scope = new AppScope();
        var env = await GetEnvironmentAsync(scope.App, "executor");

        env.Should()
           .ContainKey("EXECUTOR_SETTINGS_DIR");
        env["EXECUTOR_SETTINGS_DIR"]
           .Should()
           .NotBeNullOrWhiteSpace();
        env["EXECUTOR_SETTINGS_DIR"]!.Should()
                                     .EndWith($"Tomeshelf{Path.DirectorySeparatorChar}executor");
    }

    [Fact]
    public async Task FitbitApi_uses_configured_values()
    {
        await using var scope = new AppScope(new Dictionary<string, string?>
        {
            ["Fitbit:ApiBase"] = "https://fitbit.example/",
            ["Fitbit:UserId"] = "user-123",
            ["Fitbit:Scope"] = "activity sleep",
            ["Fitbit:CallbackPath"] = "/custom/callback",
            ["Fitbit:ClientId"] = "client-123",
            ["Fitbit:ClientSecret"] = "secret-123"
        });

        var env = await GetEnvironmentAsync(scope.App, "fitbitapi");

        env.Should()
           .ContainKey("Fitbit__ApiBase")
           .WhoseValue
           .Should()
           .Be("https://fitbit.example/");
        env.Should()
           .ContainKey("Fitbit__UserId")
           .WhoseValue
           .Should()
           .Be("user-123");
        env.Should()
           .ContainKey("Fitbit__Scope")
           .WhoseValue
           .Should()
           .Be("activity sleep");
        env.Should()
           .ContainKey("Fitbit__CallbackPath")
           .WhoseValue
           .Should()
           .Be("/custom/callback");
        env.Should()
           .ContainKey("Fitbit__ClientId")
           .WhoseValue
           .Should()
           .Be("client-123");
        env.Should()
           .ContainKey("Fitbit__ClientSecret")
           .WhoseValue
           .Should()
           .Be("secret-123");
    }

    [Fact]
    public async Task GoogleDrive_settings_flow_to_web_and_fileuploader()
    {
        await using var scope = new AppScope(new Dictionary<string, string?>
        {
            ["GoogleDrive:ClientId"] = "client-id",
            ["GoogleDrive:ClientSecret"] = "client-secret",
            ["GoogleDrive:UserEmail"] = "user@example.com",
            ["GoogleDrive:RootFolderId"] = "root-folder",
            ["GoogleDrive:RootFolderPath"] = "C:\\DriveRoot",
            ["GoogleDrive:SharedDriveId"] = "shared-drive"
        });

        var webEnv = await GetEnvironmentAsync(scope.App, "web");
        webEnv.Should()
              .ContainKey("GoogleDrive__ClientId")
              .WhoseValue
              .Should()
              .Be("client-id");
        webEnv.Should()
              .ContainKey("GoogleDrive__ClientSecret")
              .WhoseValue
              .Should()
              .Be("client-secret");
        webEnv.Should()
              .ContainKey("GoogleDrive__UserEmail")
              .WhoseValue
              .Should()
              .Be("user@example.com");
        webEnv.Should()
              .ContainKey("GoogleDrive__RootFolderId")
              .WhoseValue
              .Should()
              .Be("root-folder");
        webEnv.Keys
              .Should()
              .NotContain("GoogleDrive__RootFolderPath");
        webEnv.Keys
              .Should()
              .NotContain("GoogleDrive__SharedDriveId");

        var uploaderEnv = await GetEnvironmentAsync(scope.App, "fileuploaderapi");
        uploaderEnv.Should()
                   .ContainKey("GoogleDrive__RootFolderPath")
                   .WhoseValue
                   .Should()
                   .Be("C:\\DriveRoot");
        uploaderEnv.Should()
                   .ContainKey("GoogleDrive__RootFolderId")
                   .WhoseValue
                   .Should()
                   .Be("root-folder");
        uploaderEnv.Should()
                   .ContainKey("GoogleDrive__ClientId")
                   .WhoseValue
                   .Should()
                   .Be("client-id");
        uploaderEnv.Should()
                   .ContainKey("GoogleDrive__ClientSecret")
                   .WhoseValue
                   .Should()
                   .Be("client-secret");
        uploaderEnv.Should()
                   .ContainKey("GoogleDrive__UserEmail")
                   .WhoseValue
                   .Should()
                   .Be("user@example.com");
        uploaderEnv.Should()
                   .ContainKey("GoogleDrive__SharedDriveId")
                   .WhoseValue
                   .Should()
                   .Be("shared-drive");
    }

    [Fact]
    public async Task ShiftApi_applies_scanner_settings_and_usernames()
    {
        await using var scope = new AppScope(new Dictionary<string, string?>
        {
            ["ShiftKeyScanner:LookbackHours"] = "24",
            ["ShiftKeyScanner:X:Enabled"] = "true",
            ["ShiftKeyScanner:X:ApiBaseV2"] = "https://api.x.test",
            ["ShiftKeyScanner:X:OAuthTokenEndpoint"] = "https://auth.x.test",
            ["ShiftKeyScanner:X:BearerToken"] = "token",
            ["ShiftKeyScanner:X:ApiKey"] = "key",
            ["ShiftKeyScanner:X:ApiSecret"] = "secret",
            ["ShiftKeyScanner:X:TokenCacheMinutes"] = "5",
            ["ShiftKeyScanner:X:MaxPages"] = "10",
            ["ShiftKeyScanner:X:MaxResultsPerPage"] = "100",
            ["ShiftKeyScanner:X:ExcludeReplies"] = "true",
            ["ShiftKeyScanner:X:ExcludeRetweets"] = "false",
            ["ShiftKeyScanner:X:Usernames:0"] = "",
            ["ShiftKeyScanner:X:Usernames:1"] = "first",
            ["ShiftKeyScanner:X:Usernames:2"] = "second"
        });

        var env = await GetEnvironmentAsync(scope.App, "shiftapi");

        env.Should()
           .ContainKey("ShiftKeyScanner__LookbackHours")
           .WhoseValue
           .Should()
           .Be("24");
        env.Should()
           .ContainKey("ShiftKeyScanner__X__Enabled")
           .WhoseValue
           .Should()
           .Be("true");
        env.Should()
           .ContainKey("ShiftKeyScanner__X__ApiBaseV2")
           .WhoseValue
           .Should()
           .Be("https://api.x.test");
        env.Should()
           .ContainKey("ShiftKeyScanner__X__OAuthTokenEndpoint")
           .WhoseValue
           .Should()
           .Be("https://auth.x.test");
        env.Should()
           .ContainKey("ShiftKeyScanner__X__BearerToken")
           .WhoseValue
           .Should()
           .Be("token");
        env.Should()
           .ContainKey("ShiftKeyScanner__X__ApiKey")
           .WhoseValue
           .Should()
           .Be("key");
        env.Should()
           .ContainKey("ShiftKeyScanner__X__ApiSecret")
           .WhoseValue
           .Should()
           .Be("secret");
        env.Should()
           .ContainKey("ShiftKeyScanner__X__TokenCacheMinutes")
           .WhoseValue
           .Should()
           .Be("5");
        env.Should()
           .ContainKey("ShiftKeyScanner__X__MaxPages")
           .WhoseValue
           .Should()
           .Be("10");
        env.Should()
           .ContainKey("ShiftKeyScanner__X__MaxResultsPerPage")
           .WhoseValue
           .Should()
           .Be("100");
        env.Should()
           .ContainKey("ShiftKeyScanner__X__ExcludeReplies")
           .WhoseValue
           .Should()
           .Be("true");
        env.Should()
           .ContainKey("ShiftKeyScanner__X__ExcludeRetweets")
           .WhoseValue
           .Should()
           .Be("false");
        env.Should()
           .ContainKey("ShiftKeyScanner__X__Usernames__0")
           .WhoseValue
           .Should()
           .Be("first");
        env.Should()
           .ContainKey("ShiftKeyScanner__X__Usernames__1")
           .WhoseValue
           .Should()
           .Be("second");
        env.Keys
           .Should()
           .NotContain("ShiftKeyScanner__X__Usernames__2");
    }

    [Fact]
    public async Task SqlServer_sets_accept_eula()
    {
        await using var scope = new AppScope();
        var env = await GetEnvironmentAsync(scope.App, "sql");

        env.Should()
           .ContainKey("ACCEPT_EULA");
        env["ACCEPT_EULA"]
           .Should()
           .Be("Y");
    }

    private static async Task<IReadOnlyDictionary<string, string?>> GetEnvironmentAsync(DistributedApplication app, string resourceName)
    {
        var model = app.Services.GetRequiredService<DistributedApplicationModel>();
        var resource = model.Resources.Single(resource => string.Equals(resource.Name, resourceName, StringComparison.OrdinalIgnoreCase));
        var envResource = resource as IResourceWithEnvironment;
        envResource.Should()
                   .NotBeNull($"{resourceName} should implement IResourceWithEnvironment");

        var values = await envResource!.GetEnvironmentVariableValuesAsync(DistributedApplicationOperation.Publish);

        return values.ToDictionary(item => item.Key, item => item.Value, StringComparer.OrdinalIgnoreCase);
    }

    private sealed class AppScope : IAsyncDisposable
    {
        public AppScope(IReadOnlyDictionary<string, string?>? settings = null)
        {
            App = Program.BuildApp(Array.Empty<string>(), builder =>
            {
                if (settings is null)
                {
                    return;
                }

                builder.Configuration.AddInMemoryCollection(settings);
            });
        }

        public DistributedApplication App { get; }

        public async ValueTask DisposeAsync()
        {
            if (App is IAsyncDisposable asyncDisposable)
            {
                await asyncDisposable.DisposeAsync();

                return;
            }

            if (App is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}