using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Hosting;
using Tomeshelf.Web.Tests.TestUtilities;

namespace Tomeshelf.Web.Tests.ProgramTests;

public class OAuthRedirect
{
    [Fact]
    public async Task AddsGoogleParameters()
    {
        // Arrange
        using var app = ProgramTestHarness.BuildApp(Environments.Development, ProgramTestHarness.GoogleDriveConfig("config@example.test"));
        var options = ProgramTestHarness.GetOAuthOptions(app);
        var properties = new AuthenticationProperties();
        properties.Items["login_hint"] = "hint@example.test";

        var context = ProgramTestHarness.CreateRedirectContext(options, properties, "https://example.test/auth?existing=1");

        // Act
        await options.Events.RedirectToAuthorizationEndpoint(context);

        // Assert
        var query = ProgramTestHarness.ParseQuery(context.RedirectUri);
        query["existing"].ShouldBe("1");
        query["access_type"].ShouldBe("offline");
        query["prompt"].ShouldBe("consent");
        query["include_granted_scopes"].ShouldBe("true");
        query["login_hint"].ShouldBe("hint@example.test");
    }
}
