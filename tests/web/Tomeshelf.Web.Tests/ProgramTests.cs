using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
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
using Tomeshelf.Web.Services;
using Tomeshelf.Web.Tests.TestUtilities;

namespace Tomeshelf.Web.Tests;

public class ProgramTests
{
    private const string ClientIdKey = "gd_clientId";
    private const string ClientSecretKey = "gd_clientSecret";
    private const string RefreshTokenKey = "gd_refreshToken";
    private const string UserEmailKey = "gd_userEmail";
    private const string ErrorKey = "gd_error";

    [Fact]
    public void BuildApp_Development_UsesDefaultServiceAddresses()
    {
        var config = new Dictionary<string, string?>
        {
            ["Services:McmApiBase"] = "https://config.test/",
            ["Services:HumbleBundleApiBase"] = "https://config.test/",
            ["Services:FitbitApiBase"] = "https://config.test/",
            ["Services:PaissaApiBase"] = "https://config.test/",
            ["Services:ShiftApiBase"] = "https://config.test/"
        };

        using var app = BuildApp(Environments.Development, config);
        var factory = app.Services.GetRequiredService<IHttpClientFactory>();

        var guests = factory.CreateClient(GuestsApi.HttpClientName);
        guests.BaseAddress
              .Should()
              .Be(new Uri("https://mcmapi"));
        guests.Timeout
              .Should()
              .Be(TimeSpan.FromSeconds(100));
        guests.DefaultRequestVersion
              .Should()
              .Be(HttpVersion.Version11);
        guests.DefaultVersionPolicy
              .Should()
              .Be(HttpVersionPolicy.RequestVersionExact);

        var bundles = factory.CreateClient(BundlesApi.HttpClientName);
        bundles.BaseAddress
               .Should()
               .Be(new Uri("https://humblebundleapi"));
        bundles.Timeout
               .Should()
               .Be(TimeSpan.FromSeconds(100));

        var fitbit = factory.CreateClient(FitbitApi.HttpClientName);
        fitbit.BaseAddress
              .Should()
              .Be(new Uri("https://fitbitapi"));
        fitbit.Timeout
              .Should()
              .Be(TimeSpan.FromSeconds(100));

        var paissa = factory.CreateClient(PaissaApi.HttpClientName);
        paissa.BaseAddress
              .Should()
              .Be(new Uri("https://paissaapi"));
        paissa.Timeout
              .Should()
              .Be(TimeSpan.FromSeconds(30));

        var shift = factory.CreateClient(ShiftApi.HttpClientName);
        shift.BaseAddress
             .Should()
             .Be(new Uri("https://shiftapi"));
        shift.Timeout
             .Should()
             .Be(TimeSpan.FromSeconds(30));

        var uploads = factory.CreateClient(FileUploadsApi.HttpClientName);
        uploads.BaseAddress
               .Should()
               .Be(new Uri("https://localhost:49960"));
        uploads.Timeout
               .Should()
               .Be(TimeSpan.FromMinutes(30));
    }

    [Fact]
    public void BuildApp_Production_UsesApiBaseFallbackForGuests()
    {
        var config = new Dictionary<string, string?>
        {
            ["Services:McmApiBase"] = null,
            ["Services:ApiBase"] = "https://fallback.example.test/"
        };

        using var app = BuildApp(Environments.Production, config);
        var client = app.Services
                        .GetRequiredService<IHttpClientFactory>()
                        .CreateClient(GuestsApi.HttpClientName);

        client.BaseAddress
              .Should()
              .Be(new Uri("https://fallback.example.test/"));
    }

    [Fact]
    public void BuildApp_Production_UsesConfiguredServiceAddresses()
    {
        var config = new Dictionary<string, string?>
        {
            ["Services:McmApiBase"] = "https://mcm.example.test/",
            ["Services:HumbleBundleApiBase"] = "https://humble.example.test/",
            ["Services:FitbitApiBase"] = "https://fitbit.example.test/",
            ["Services:PaissaApiBase"] = "https://paissa.example.test/",
            ["Services:ShiftApiBase"] = "https://shift.example.test/",
            ["Services:FileUploaderApiBase"] = "https://uploads.example.test/"
        };

        using var app = BuildApp(Environments.Production, config);
        var factory = app.Services.GetRequiredService<IHttpClientFactory>();

        factory.CreateClient(GuestsApi.HttpClientName)
               .BaseAddress
               .Should()
               .Be(new Uri("https://mcm.example.test/"));
        factory.CreateClient(BundlesApi.HttpClientName)
               .BaseAddress
               .Should()
               .Be(new Uri("https://humble.example.test/"));
        factory.CreateClient(FitbitApi.HttpClientName)
               .BaseAddress
               .Should()
               .Be(new Uri("https://fitbit.example.test/"));
        factory.CreateClient(PaissaApi.HttpClientName)
               .BaseAddress
               .Should()
               .Be(new Uri("https://paissa.example.test/"));
        factory.CreateClient(ShiftApi.HttpClientName)
               .BaseAddress
               .Should()
               .Be(new Uri("https://shift.example.test/"));
        factory.CreateClient(FileUploadsApi.HttpClientName)
               .BaseAddress
               .Should()
               .Be(new Uri("https://uploads.example.test/"));
    }

    [Theory]
    [MemberData(nameof(InvalidServiceUris))]
    public void BuildApp_WhenInvalidUriConfigured_Throws(string key, string clientName, string message)
    {
        var config = new Dictionary<string, string?> { [key] = "not-a-uri" };

        using var app = BuildApp(Environments.Production, config);
        var factory = app.Services.GetRequiredService<IHttpClientFactory>();

        Action act = () => factory.CreateClient(clientName);
        act.Should()
           .Throw<InvalidOperationException>()
           .WithMessage(message);
    }

    public static IEnumerable<object[]> InvalidServiceUris()
    {
        yield return new object[] { "Services:McmApiBase", GuestsApi.HttpClientName, "Invalid URI in configuration setting 'Services:McmApiBase'." };
        yield return new object[] { "Services:HumbleBundleApiBase", BundlesApi.HttpClientName, "Invalid URI in configuration setting 'Services:HumbleBundleApiBase'." };
        yield return new object[] { "Services:FitbitApiBase", FitbitApi.HttpClientName, "Invalid URI in configuration setting 'Services:FitbitApiBase'." };
        yield return new object[] { "Services:PaissaApiBase", PaissaApi.HttpClientName, "Invalid URI in configuration setting 'Services:PaissaApiBase'." };
        yield return new object[] { "Services:ShiftApiBase", ShiftApi.HttpClientName, "Invalid URI in configuration setting 'Services:ShiftApiBase'." };
        yield return new object[] { "Services:FileUploaderApiBase", FileUploadsApi.HttpClientName, "Invalid URI in configuration setting 'Services:FileUploaderApiBase'." };
    }

    [Fact]
    public async Task OAuthCreatingTicket_SetsSessionValuesFromLoginHint()
    {
        using var app = BuildApp(Environments.Development, GoogleDriveConfig("config@example.test"));
        var options = GetOAuthOptions(app);
        var (httpContext, _) = CreateSessionContext();
        var properties = new AuthenticationProperties();
        properties.Items["login_hint"] = "hint@example.test";

        using var tokenDoc = JsonDocument.Parse("{\"access_token\":\"token\",\"refresh_token\":\"refresh\"}");
        using var userDoc = JsonDocument.Parse("{}");
        var tokenResponse = OAuthTokenResponse.Success(tokenDoc);

        var context = new OAuthCreatingTicketContext(new ClaimsPrincipal(new ClaimsIdentity()), properties, httpContext, CreateScheme(), options, new HttpClient(), tokenResponse, userDoc.RootElement);

        await options.Events.CreatingTicket(context);

        httpContext.Session
                   .GetString(ClientIdKey)
                   .Should()
                   .Be("client-id");
        httpContext.Session
                   .GetString(ClientSecretKey)
                   .Should()
                   .Be("client-secret");
        httpContext.Session
                   .GetString(RefreshTokenKey)
                   .Should()
                   .Be("refresh");
        httpContext.Session
                   .GetString(UserEmailKey)
                   .Should()
                   .Be("hint@example.test");
    }

    [Fact]
    public async Task OAuthCreatingTicket_UsesConfiguredEmailWhenNoLoginHint()
    {
        using var app = BuildApp(Environments.Development, GoogleDriveConfig("config@example.test"));
        var options = GetOAuthOptions(app);
        var (httpContext, _) = CreateSessionContext();

        using var tokenDoc = JsonDocument.Parse("{\"access_token\":\"token\",\"refresh_token\":\"refresh\"}");
        using var userDoc = JsonDocument.Parse("{}");
        var tokenResponse = OAuthTokenResponse.Success(tokenDoc);

        var context = new OAuthCreatingTicketContext(new ClaimsPrincipal(new ClaimsIdentity()), new AuthenticationProperties(), httpContext, CreateScheme(), options, new HttpClient(), tokenResponse, userDoc.RootElement);

        await options.Events.CreatingTicket(context);

        httpContext.Session
                   .GetString(UserEmailKey)
                   .Should()
                   .Be("config@example.test");
    }

    [Fact]
    public async Task OAuthCreatingTicket_WhenRefreshTokenMissing_SetsError()
    {
        using var app = BuildApp(Environments.Development, GoogleDriveConfig("config@example.test"));
        var options = GetOAuthOptions(app);
        var (httpContext, _) = CreateSessionContext();

        using var tokenDoc = JsonDocument.Parse("{\"access_token\":\"token\"}");
        using var userDoc = JsonDocument.Parse("{}");
        var tokenResponse = OAuthTokenResponse.Success(tokenDoc);

        var context = new OAuthCreatingTicketContext(new ClaimsPrincipal(new ClaimsIdentity()), new AuthenticationProperties(), httpContext, CreateScheme(), options, new HttpClient(), tokenResponse, userDoc.RootElement);

        await options.Events.CreatingTicket(context);

        httpContext.Session
                   .GetString(ErrorKey)
                   .Should()
                   .Be("Google did not return a refresh token. Re-run the flow and accept offline access.");
        context.Result
              ?.Failure
              ?.Message
               .Should()
               .Be("Missing refresh token.");
    }

    [Fact]
    public async Task OAuthRedirect_AddsGoogleParameters()
    {
        using var app = BuildApp(Environments.Development, GoogleDriveConfig("config@example.test"));
        var options = GetOAuthOptions(app);
        var properties = new AuthenticationProperties();
        properties.Items["login_hint"] = "hint@example.test";

        var context = new RedirectContext<OAuthOptions>(new DefaultHttpContext(), CreateScheme(), options, properties, "https://example.test/auth?existing=1");

        await options.Events.RedirectToAuthorizationEndpoint(context);

        var uri = new Uri(context.RedirectUri);
        var query = QueryHelpers.ParseQuery(uri.Query);
        query["existing"]
           .ToString()
           .Should()
           .Be("1");
        query["access_type"]
           .ToString()
           .Should()
           .Be("offline");
        query["prompt"]
           .ToString()
           .Should()
           .Be("consent");
        query["include_granted_scopes"]
           .ToString()
           .Should()
           .Be("true");
        query["login_hint"]
           .ToString()
           .Should()
           .Be("hint@example.test");
    }

    [Fact]
    public async Task OAuthRemoteFailure_WritesSessionAndRedirects()
    {
        using var app = BuildApp(Environments.Development, GoogleDriveConfig("config@example.test"));
        var options = GetOAuthOptions(app);
        var (httpContext, _) = CreateSessionContext();

        var context = new RemoteFailureContext(httpContext, CreateScheme(), options, new InvalidOperationException("boom"));

        await options.Events.RemoteFailure(context);

        httpContext.Session
                   .GetString(ErrorKey)
                   .Should()
                   .Be("boom");
        httpContext.Response
                   .StatusCode
                   .Should()
                   .Be(StatusCodes.Status302Found);
        httpContext.Response
                   .Headers
                   .Location
                   .ToString()
                   .Should()
                   .Be("/drive-auth/result");
    }

    private static WebApplication BuildApp(string environment, Dictionary<string, string?>? config = null)
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

    private static AuthenticationScheme CreateScheme()
    {
        return new AuthenticationScheme(DriveAuthController.AuthenticationScheme, DriveAuthController.AuthenticationScheme, typeof(OAuthHandler<OAuthOptions>));
    }

    private static (DefaultHttpContext Context, TestSession Session) CreateSessionContext()
    {
        var session = new TestSession();
        var context = new DefaultHttpContext();
        context.Features.Set<ISessionFeature>(new TestSessionFeature { Session = session });

        return (context, session);
    }

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

    private static OAuthOptions GetOAuthOptions(WebApplication app)
    {
        var monitor = app.Services.GetRequiredService<IOptionsMonitor<OAuthOptions>>();

        return monitor.Get(DriveAuthController.AuthenticationScheme);
    }

    private static Dictionary<string, string?> GoogleDriveConfig(string userEmail)
    {
        return new Dictionary<string, string?>
        {
            ["GoogleDrive:ClientId"] = "client-id",
            ["GoogleDrive:ClientSecret"] = "client-secret",
            ["GoogleDrive:UserEmail"] = userEmail
        };
    }

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