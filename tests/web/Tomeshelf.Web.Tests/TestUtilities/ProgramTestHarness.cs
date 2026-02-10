using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tomeshelf.Web.Controllers;

namespace Tomeshelf.Web.Tests.TestUtilities;

public static class ProgramTestHarness
{
    public const string ClientIdKey = "gd_clientId";
    public const string ClientSecretKey = "gd_clientSecret";
    public const string RefreshTokenKey = "gd_refreshToken";
    public const string UserEmailKey = "gd_userEmail";
    public const string ErrorKey = "gd_error";

    /// <summary>
    ///     Builds the app.
    /// </summary>
    /// <param name="environment">The environment.</param>
    /// <param name="config">The config.</param>
    /// <returns>The result of the operation.</returns>
    public static WebApplication BuildApp(string environment, Dictionary<string, string?>? config = null)
    {
        return Program.BuildApp(Array.Empty<string>(), builder =>
        {
            builder.Environment.EnvironmentName = environment;
            builder.Logging.ClearProviders();

            var settings = new Dictionary<string, string?>(StringComparer.Ordinal);
            if (config is not null)
            {
                foreach (var entry in config)
                {
                    settings[entry.Key] = entry.Value;
                }
            }

            settings["StaticAssets:ManifestPath"] = ResolveStaticAssetsManifestPath();
            builder.Configuration.AddInMemoryCollection(settings);
        });
    }

    /// <summary>
    ///     Creates the scheme.
    /// </summary>
    /// <returns>The result of the operation.</returns>
    public static AuthenticationScheme CreateScheme()
    {
        return new AuthenticationScheme(DriveAuthController.AuthenticationScheme, DriveAuthController.AuthenticationScheme, typeof(OAuthHandler<OAuthOptions>));
    }

    /// <summary>
    ///     Creates the session context.
    /// </summary>
    /// <returns>The result of the operation.</returns>
    public static (DefaultHttpContext Context, TestSession Session) CreateSessionContext()
    {
        var session = new TestSession();
        var context = new DefaultHttpContext();
        context.Features.Set<ISessionFeature>(new TestSessionFeature { Session = session });

        return (context, session);
    }

    /// <summary>
    ///     GetOs the auth options.
    /// </summary>
    /// <param name="app">The app.</param>
    /// <returns>The result of the operation.</returns>
    public static OAuthOptions GetOAuthOptions(WebApplication app)
    {
        var monitor = app.Services.GetRequiredService<IOptionsMonitor<OAuthOptions>>();

        return monitor.Get(DriveAuthController.AuthenticationScheme);
    }

    /// <summary>
    ///     Googles the drive config.
    /// </summary>
    /// <param name="userEmail">The user email.</param>
    /// <returns>The result of the operation.</returns>
    public static Dictionary<string, string?> GoogleDriveConfig(string userEmail)
    {
        return new Dictionary<string, string?>
        {
            ["GoogleDrive:ClientId"] = "client-id",
            ["GoogleDrive:ClientSecret"] = "client-secret",
            ["GoogleDrive:UserEmail"] = userEmail
        };
    }

    /// <summary>
    ///     CreateOs the auth context.
    /// </summary>
    /// <param name="httpContext">The http context.</param>
    /// <param name="options">The options.</param>
    /// <param name="properties">The properties.</param>
    /// <param name="tokenDoc">The token doc.</param>
    /// <param name="userDoc">The user doc.</param>
    /// <returns>The result of the operation.</returns>
    public static OAuthCreatingTicketContext CreateOAuthContext(DefaultHttpContext httpContext, OAuthOptions options, AuthenticationProperties? properties = null, JsonDocument? tokenDoc = null, JsonDocument? userDoc = null)
    {
        var tokenPayload = tokenDoc ?? JsonDocument.Parse("{\"access_token\":\"token\",\"refresh_token\":\"refresh\"}");
        var userPayload = userDoc ?? JsonDocument.Parse("{}");
        var tokenResponse = OAuthTokenResponse.Success(tokenPayload);
        var authProperties = properties ?? new AuthenticationProperties();

        return new OAuthCreatingTicketContext(new ClaimsPrincipal(new ClaimsIdentity()), authProperties, httpContext, CreateScheme(), options, new HttpClient(), tokenResponse, userPayload.RootElement);
    }

    /// <summary>
    ///     Creates the redirect context.
    /// </summary>
    /// <param name="options">The options.</param>
    /// <param name="properties">The properties.</param>
    /// <param name="redirectUri">The redirect uri.</param>
    /// <returns>The result of the operation.</returns>
    public static RedirectContext<OAuthOptions> CreateRedirectContext(OAuthOptions options, AuthenticationProperties properties, string redirectUri)
    {
        return new RedirectContext<OAuthOptions>(new DefaultHttpContext(), CreateScheme(), options, properties, redirectUri);
    }

    /// <summary>
    ///     Creates the remote failure context.
    /// </summary>
    /// <param name="httpContext">The http context.</param>
    /// <param name="options">The options.</param>
    /// <param name="exception">The exception.</param>
    /// <returns>The result of the operation.</returns>
    public static RemoteFailureContext CreateRemoteFailureContext(DefaultHttpContext httpContext, OAuthOptions options, Exception exception)
    {
        return new RemoteFailureContext(httpContext, CreateScheme(), options, exception);
    }

    /// <summary>
    ///     Parses the query.
    /// </summary>
    /// <param name="uri">The uri.</param>
    /// <returns>The result of the operation.</returns>
    public static Dictionary<string, string> ParseQuery(string uri)
    {
        var parsed = QueryHelpers.ParseQuery(new Uri(uri).Query);
        return parsed.ToDictionary(pair => pair.Key, pair => pair.Value.ToString(), StringComparer.Ordinal);
    }

    /// <summary>
    ///     Finds the repo root.
    /// </summary>
    /// <returns>The resulting string.</returns>
    private static string FindRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Tomeshelf.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Repository root not found from test base directory.");
    }

    /// <summary>
    ///     Resolves the static assets manifest path.
    /// </summary>
    /// <returns>The resulting string.</returns>
    private static string ResolveStaticAssetsManifestPath()
    {
        var root = FindRepoRoot();
        var debugPath = Path.Combine(root, "src", "web", "Tomeshelf.Web", "bin", "Debug", "net10.0", "Tomeshelf.Web.staticwebassets.endpoints.json");
        if (File.Exists(debugPath))
        {
            return debugPath;
        }

        var releasePath = Path.Combine(root, "src", "web", "Tomeshelf.Web", "bin", "Release", "net10.0", "Tomeshelf.Web.staticwebassets.endpoints.json");
        if (File.Exists(releasePath))
        {
            return releasePath;
        }

        throw new InvalidOperationException($"Static web assets manifest not found at '{debugPath}' or '{releasePath}'.");
    }

    private sealed class TestSessionFeature : ISessionFeature
    {
        public ISession Session { get; set; } = default!;
    }
}
