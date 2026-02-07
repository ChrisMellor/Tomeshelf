using System.Collections.Generic;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Session;
using Microsoft.Extensions.Configuration;
using Tomeshelf.Web.Controllers;
using Tomeshelf.Web.Models.DriveAuth;
using Tomeshelf.Web.Tests.TestUtilities;

namespace Tomeshelf.Web.Tests.Controllers.DriveAuthControllerTests;

public class Start
{
    private const string ReturnUrlKey = "gd_returnUrl";

    [Fact]
    public void WhenConfigMissing_ReturnsOAuthResultView()
    {
        // Arrange
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>())
                                               .Build();
        var controller = new DriveAuthController(config) { ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() } };

        // Act
        var result = controller.Start();

        // Assert
        var view = result.ShouldBeOfType<ViewResult>();
        view.ViewName.ShouldBe("OAuthResult");
        var model = view.Model.ShouldBeOfType<OAuthResultViewModel>();
        model.Success.ShouldBeFalse();
        model.Message.ShouldContain("Google Drive OAuth is not configured");
    }

    [Fact]
    public void WhenConfigPresent_SetsReturnUrlAndChallenges()
    {
        // Arrange
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
                                                {
                                                    ["GoogleDrive:ClientId"] = "client",
                                                    ["GoogleDrive:ClientSecret"] = "secret",
                                                    ["GoogleDrive:UserEmail"] = "user@example.com"
                                                })
                                               .Build();

        var session = new TestSession();
        var httpContext = new DefaultHttpContext();
        httpContext.Features.Set<ISessionFeature>(new SessionFeature { Session = session });

        var urlHelper = A.Fake<IUrlHelper>();
        A.CallTo(() => urlHelper.Action(A<UrlActionContext>._))
         .Returns("https://example.test/drive-auth/result");

        var controller = new DriveAuthController(config)
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext },
            Url = urlHelper
        };

        // Act
        var result = controller.Start("https://example.test/return");

        // Assert
        var challenge = result.ShouldBeOfType<ChallengeResult>();
        var scheme = challenge.AuthenticationSchemes.ShouldHaveSingleItem();
        scheme.ShouldBe(DriveAuthController.AuthenticationScheme);
        challenge.Properties?.RedirectUri.ShouldBe("https://example.test/drive-auth/result");
        challenge.Properties.ShouldNotBeNull();
        challenge.Properties!.Items.ContainsKey("login_hint").ShouldBeTrue();
        challenge.Properties.Items["login_hint"].ShouldBe("user@example.com");
        httpContext.Session.GetString(ReturnUrlKey).ShouldBe("https://example.test/return");
    }
}
