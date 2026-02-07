using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Shouldly;
using Tomeshelf.Web.Tests.TestUtilities;

namespace Tomeshelf.Web.Tests.ProgramTests;

public class OAuthCreatingTicket
{
    [Fact]
    public async Task SetsSessionValuesFromLoginHint()
    {
        using var app = ProgramTestHarness.BuildApp(Environments.Development, ProgramTestHarness.GoogleDriveConfig("config@example.test"));
        var options = ProgramTestHarness.GetOAuthOptions(app);
        var (httpContext, _) = ProgramTestHarness.CreateSessionContext();
        var properties = new AuthenticationProperties();
        properties.Items["login_hint"] = "hint@example.test";

        using var tokenDoc = JsonDocument.Parse("{\"access_token\":\"token\",\"refresh_token\":\"refresh\"}");
        using var userDoc = JsonDocument.Parse("{}");
        var context = ProgramTestHarness.CreateOAuthContext(httpContext, options, properties, tokenDoc, userDoc);

        await options.Events.CreatingTicket(context);

        httpContext.Session
                   .GetString(ProgramTestHarness.ClientIdKey)
                   .ShouldBe("client-id");
        httpContext.Session
                   .GetString(ProgramTestHarness.ClientSecretKey)
                   .ShouldBe("client-secret");
        httpContext.Session
                   .GetString(ProgramTestHarness.RefreshTokenKey)
                   .ShouldBe("refresh");
        httpContext.Session
                   .GetString(ProgramTestHarness.UserEmailKey)
                   .ShouldBe("hint@example.test");
    }

    [Fact]
    public async Task UsesConfiguredEmailWhenNoLoginHint()
    {
        using var app = ProgramTestHarness.BuildApp(Environments.Development, ProgramTestHarness.GoogleDriveConfig("config@example.test"));
        var options = ProgramTestHarness.GetOAuthOptions(app);
        var (httpContext, _) = ProgramTestHarness.CreateSessionContext();

        using var tokenDoc = JsonDocument.Parse("{\"access_token\":\"token\",\"refresh_token\":\"refresh\"}");
        using var userDoc = JsonDocument.Parse("{}");
        var context = ProgramTestHarness.CreateOAuthContext(httpContext, options, null, tokenDoc, userDoc);

        await options.Events.CreatingTicket(context);

        httpContext.Session
                   .GetString(ProgramTestHarness.UserEmailKey)
                   .ShouldBe("config@example.test");
    }

    [Fact]
    public async Task WhenRefreshTokenMissing_SetsError()
    {
        using var app = ProgramTestHarness.BuildApp(Environments.Development, ProgramTestHarness.GoogleDriveConfig("config@example.test"));
        var options = ProgramTestHarness.GetOAuthOptions(app);
        var (httpContext, _) = ProgramTestHarness.CreateSessionContext();

        using var tokenDoc = JsonDocument.Parse("{\"access_token\":\"token\"}");
        using var userDoc = JsonDocument.Parse("{}");
        var context = ProgramTestHarness.CreateOAuthContext(httpContext, options, null, tokenDoc, userDoc);

        await options.Events.CreatingTicket(context);

        httpContext.Session
                   .GetString(ProgramTestHarness.ErrorKey)
                   .ShouldBe("Google did not return a refresh token. Re-run the flow and accept offline access.");
        context.Result?.Failure?.Message.ShouldBe("Missing refresh token.");
    }
}