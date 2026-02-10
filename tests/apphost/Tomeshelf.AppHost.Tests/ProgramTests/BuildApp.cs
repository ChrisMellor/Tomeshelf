using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Tomeshelf.AppHost.Tests.ProgramTests;

public class BuildApp
{
    /// <summary>
    ///     Adds the expected resources.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task AddsExpectedResources()
    {
        // Arrange

        await using var scope = new AppScope();
        var model = scope.App.Services.GetRequiredService<DistributedApplicationModel>();
        var expected = new[] { "sql", "mcmdb", "humblebundledb", "fitbitdb", "shiftdb", "mcmapi", "humblebundleapi", "fitbitapi", "paissaapi", "fileuploaderapi", "shiftapi", "executor", "web" };

        var names = model.Resources
                         .Select(resource => resource.Name)
                         .ToArray();

        foreach (var name in expected)
        {
            names.ShouldContain(name);
        }
    }

    /// <summary>
    ///     Applies the fitbit values.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task AppliesFitbitValues()
    {
        // Arrange
        await using var scope = new AppScope(new Dictionary<string, string?>
        {
            ["Fitbit:ApiBase"] = "https://fitbit.example/",
            ["Fitbit:UserId"] = "user-123",
            ["Fitbit:Scope"] = "activity sleep",
            ["Fitbit:CallbackPath"] = "/custom/callback",
            ["Fitbit:ClientId"] = "client-123",
            ["Fitbit:ClientSecret"] = "secret-123"
        });

        // Act
        var env = await GetEnvironmentAsync(scope.App, "fitbitapi");

        // Assert
        env.TryGetValue("Fitbit__ApiBase", out var apiBase)
           .ShouldBeTrue();
        apiBase.ShouldBe("https://fitbit.example/");
        env.TryGetValue("Fitbit__UserId", out var userId)
           .ShouldBeTrue();
        userId.ShouldBe("user-123");
        env.TryGetValue("Fitbit__Scope", out var scopeValue)
           .ShouldBeTrue();
        scopeValue.ShouldBe("activity sleep");
        env.TryGetValue("Fitbit__CallbackPath", out var callbackPath)
           .ShouldBeTrue();
        callbackPath.ShouldBe("/custom/callback");
        env.TryGetValue("Fitbit__ClientId", out var clientId)
           .ShouldBeTrue();
        clientId.ShouldBe("client-123");
        env.TryGetValue("Fitbit__ClientSecret", out var clientSecret)
           .ShouldBeTrue();
        clientSecret.ShouldBe("secret-123");
    }

    /// <summary>
    ///     Applies the shift scanner settings and usernames.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task AppliesShiftScannerSettingsAndUsernames()
    {
        // Arrange
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

        // Act
        var env = await GetEnvironmentAsync(scope.App, "shiftapi");

        // Assert
        env.TryGetValue("ShiftKeyScanner__LookbackHours", out var lookbackHours)
           .ShouldBeTrue();
        lookbackHours.ShouldBe("24");
        env.TryGetValue("ShiftKeyScanner__X__Enabled", out var enabled)
           .ShouldBeTrue();
        enabled.ShouldBe("true");
        env.TryGetValue("ShiftKeyScanner__X__ApiBaseV2", out var apiBase)
           .ShouldBeTrue();
        apiBase.ShouldBe("https://api.x.test");
        env.TryGetValue("ShiftKeyScanner__X__OAuthTokenEndpoint", out var oauthEndpoint)
           .ShouldBeTrue();
        oauthEndpoint.ShouldBe("https://auth.x.test");
        env.TryGetValue("ShiftKeyScanner__X__BearerToken", out var bearerToken)
           .ShouldBeTrue();
        bearerToken.ShouldBe("token");
        env.TryGetValue("ShiftKeyScanner__X__ApiKey", out var apiKey)
           .ShouldBeTrue();
        apiKey.ShouldBe("key");
        env.TryGetValue("ShiftKeyScanner__X__ApiSecret", out var apiSecret)
           .ShouldBeTrue();
        apiSecret.ShouldBe("secret");
        env.TryGetValue("ShiftKeyScanner__X__TokenCacheMinutes", out var tokenCacheMinutes)
           .ShouldBeTrue();
        tokenCacheMinutes.ShouldBe("5");
        env.TryGetValue("ShiftKeyScanner__X__MaxPages", out var maxPages)
           .ShouldBeTrue();
        maxPages.ShouldBe("10");
        env.TryGetValue("ShiftKeyScanner__X__MaxResultsPerPage", out var maxResults)
           .ShouldBeTrue();
        maxResults.ShouldBe("100");
        env.TryGetValue("ShiftKeyScanner__X__ExcludeReplies", out var excludeReplies)
           .ShouldBeTrue();
        excludeReplies.ShouldBe("true");
        env.TryGetValue("ShiftKeyScanner__X__ExcludeRetweets", out var excludeRetweets)
           .ShouldBeTrue();
        excludeRetweets.ShouldBe("false");
        env.TryGetValue("ShiftKeyScanner__X__Usernames__0", out var firstUsername)
           .ShouldBeTrue();
        firstUsername.ShouldBe("first");
        env.TryGetValue("ShiftKeyScanner__X__Usernames__1", out var secondUsername)
           .ShouldBeTrue();
        secondUsername.ShouldBe("second");
        env.ContainsKey("ShiftKeyScanner__X__Usernames__2")
           .ShouldBeFalse();
    }

    /// <summary>
    ///     Flows the google drive settings.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task FlowsGoogleDriveSettings()
    {
        // Arrange
        await using var scope = new AppScope(new Dictionary<string, string?>
        {
            ["GoogleDrive:ClientId"] = "client-id",
            ["GoogleDrive:ClientSecret"] = "client-secret",
            ["GoogleDrive:UserEmail"] = "user@example.com",
            ["GoogleDrive:RootFolderId"] = "root-folder",
            ["GoogleDrive:RootFolderPath"] = "C:\\DriveRoot",
            ["GoogleDrive:SharedDriveId"] = "shared-drive"
        });

        // Act
        var webEnv = await GetEnvironmentAsync(scope.App, "web");
        var uploaderEnv = await GetEnvironmentAsync(scope.App, "fileuploaderapi");

        // Assert
        webEnv.TryGetValue("GoogleDrive__ClientId", out var webClientId)
              .ShouldBeTrue();
        webClientId.ShouldBe("client-id");
        webEnv.TryGetValue("GoogleDrive__ClientSecret", out var webClientSecret)
              .ShouldBeTrue();
        webClientSecret.ShouldBe("client-secret");
        webEnv.TryGetValue("GoogleDrive__UserEmail", out var webUserEmail)
              .ShouldBeTrue();
        webUserEmail.ShouldBe("user@example.com");
        webEnv.TryGetValue("GoogleDrive__RootFolderId", out var webRootFolderId)
              .ShouldBeTrue();
        webRootFolderId.ShouldBe("root-folder");
        webEnv.ContainsKey("GoogleDrive__RootFolderPath")
              .ShouldBeFalse();
        webEnv.ContainsKey("GoogleDrive__SharedDriveId")
              .ShouldBeFalse();

        uploaderEnv.TryGetValue("GoogleDrive__RootFolderPath", out var uploaderRootPath)
                   .ShouldBeTrue();
        uploaderRootPath.ShouldBe("C:\\DriveRoot");
        uploaderEnv.TryGetValue("GoogleDrive__RootFolderId", out var uploaderRootId)
                   .ShouldBeTrue();
        uploaderRootId.ShouldBe("root-folder");
        uploaderEnv.TryGetValue("GoogleDrive__ClientId", out var uploaderClientId)
                   .ShouldBeTrue();
        uploaderClientId.ShouldBe("client-id");
        uploaderEnv.TryGetValue("GoogleDrive__ClientSecret", out var uploaderClientSecret)
                   .ShouldBeTrue();
        uploaderClientSecret.ShouldBe("client-secret");
        uploaderEnv.TryGetValue("GoogleDrive__UserEmail", out var uploaderUserEmail)
                   .ShouldBeTrue();
        uploaderUserEmail.ShouldBe("user@example.com");
        uploaderEnv.TryGetValue("GoogleDrive__SharedDriveId", out var uploaderSharedDriveId)
                   .ShouldBeTrue();
        uploaderSharedDriveId.ShouldBe("shared-drive");
    }

    /// <summary>
    ///     Sets the executor settings directory.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task SetsExecutorSettingsDirectory()
    {
        // Arrange
        await using var scope = new AppScope();

        // Act
        var env = await GetEnvironmentAsync(scope.App, "executor");

        // Assert
        env.TryGetValue("EXECUTOR_SETTINGS_DIR", out var settingsDir)
           .ShouldBeTrue();
        string.IsNullOrWhiteSpace(settingsDir)
              .ShouldBeFalse();
        settingsDir.ShouldEndWith($"Tomeshelf{Path.DirectorySeparatorChar}executor");
    }

    /// <summary>
    ///     Sets the sql server accept eula.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task SetsSqlServerAcceptEula()
    {
        // Arrange
        await using var scope = new AppScope();

        // Act
        var env = await GetEnvironmentAsync(scope.App, "sql");

        // Assert
        env.TryGetValue("ACCEPT_EULA", out var acceptEula)
           .ShouldBeTrue();
        acceptEula.ShouldBe("Y");
    }

    /// <summary>
    ///     Gets the environment asynchronously.
    /// </summary>
    /// <param name="app">The app.</param>
    /// <param name="resourceName">The resource name.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the operation result.</returns>
    private static async Task<IReadOnlyDictionary<string, string?>> GetEnvironmentAsync(DistributedApplication app, string resourceName)
    {
        var model = app.Services.GetRequiredService<DistributedApplicationModel>();
        var resource = model.Resources.Single(resource => string.Equals(resource.Name, resourceName, StringComparison.OrdinalIgnoreCase));
        var envResource = resource as IResourceWithEnvironment;
        envResource.ShouldNotBeNull();

        var values = await envResource!.GetEnvironmentVariableValuesAsync(DistributedApplicationOperation.Publish);

        return values.ToDictionary(item => item.Key, item => item.Value, StringComparer.OrdinalIgnoreCase);
    }

    private sealed class AppScope : IAsyncDisposable
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="AppScope" /> class.
        /// </summary>
        /// <param name="settings">The settings.</param>
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

        /// <summary>
        ///     Asynchronously releases resources used by this instance.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
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