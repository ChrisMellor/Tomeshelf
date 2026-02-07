using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Session;
using Microsoft.Extensions.Configuration;
using Tomeshelf.Web.Controllers;
using Tomeshelf.Web.Models.DriveAuth;
using Tomeshelf.Web.Tests.TestUtilities;

namespace Tomeshelf.Web.Tests.Controllers.DriveAuthControllerTests;

public class Result
{
    private const string ClientIdKey = "gd_clientId";
    private const string ClientSecretKey = "gd_clientSecret";
    private const string RefreshTokenKey = "gd_refreshToken";
    private const string UserEmailKey = "gd_userEmail";
    private const string ReturnUrlKey = "gd_returnUrl";
    private const string ErrorKey = "gd_error";

    [Fact]
    public void WhenErrorInSession_ReturnsErrorViewAndClearsSession()
    {
        // Arrange
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>())
                                               .Build();
        var session = new TestSession();
        session.SetString(ErrorKey, "boom");
        var httpContext = new DefaultHttpContext();
        httpContext.Features.Set<ISessionFeature>(new SessionFeature { Session = session });

        var controller = new DriveAuthController(config) { ControllerContext = new ControllerContext { HttpContext = httpContext } };

        // Act
        var result = controller.Result();

        // Assert
        var view = result.ShouldBeOfType<ViewResult>();
        view.ViewName.ShouldBe("OAuthResult");
        var model = view.Model.ShouldBeOfType<OAuthResultViewModel>();
        model.Success.ShouldBeFalse();
        model.Message.ShouldBe("boom");
        httpContext.Session.GetString(ErrorKey).ShouldBeNull();
    }

    [Fact]
    public void WhenReturnUrlProvided_OverridesSessionReturnUrl()
    {
        // Arrange
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>())
                                               .Build();
        var session = new TestSession();
        session.SetString(ClientIdKey, "client");
        session.SetString(ClientSecretKey, "secret");
        session.SetString(RefreshTokenKey, "refresh");
        session.SetString(ReturnUrlKey, "https://example.test/from-session");

        var httpContext = new DefaultHttpContext();
        httpContext.Features.Set<ISessionFeature>(new SessionFeature { Session = session });

        var controller = new DriveAuthController(config) { ControllerContext = new ControllerContext { HttpContext = httpContext } };

        // Act
        var result = controller.Result("https://example.test/from-query");

        // Assert
        var view = result.ShouldBeOfType<ViewResult>();
        var model = view.Model.ShouldBeOfType<OAuthResultViewModel>();
        model.RedirectUrl.ShouldBe("https://example.test/from-query");
        httpContext.Session.GetString(ReturnUrlKey).ShouldBeNull();
    }

    [Fact]
    public void WhenTokensMissing_ReturnsFailureView()
    {
        // Arrange
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>())
                                               .Build();
        var session = new TestSession();
        var httpContext = new DefaultHttpContext();
        httpContext.Features.Set<ISessionFeature>(new SessionFeature { Session = session });

        var controller = new DriveAuthController(config) { ControllerContext = new ControllerContext { HttpContext = httpContext } };

        // Act
        var result = controller.Result();

        // Assert
        var view = result.ShouldBeOfType<ViewResult>();
        var model = view.Model.ShouldBeOfType<OAuthResultViewModel>();
        model.Success.ShouldBeFalse();
        model.Message.ShouldBe("Google authorisation did not complete. Please try again.");
    }

    [Fact]
    public void WhenTokensPresent_ReturnsSuccessViewAndUsesReturnUrl()
    {
        // Arrange
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>())
                                               .Build();
        var session = new TestSession();
        session.SetString(ClientIdKey, "client");
        session.SetString(ClientSecretKey, "secret");
        session.SetString(RefreshTokenKey, "refresh");
        session.SetString(UserEmailKey, "user@example.com");
        session.SetString(ReturnUrlKey, "https://example.test/return");

        var httpContext = new DefaultHttpContext();
        httpContext.Features.Set<ISessionFeature>(new SessionFeature { Session = session });

        var controller = new DriveAuthController(config) { ControllerContext = new ControllerContext { HttpContext = httpContext } };

        // Act
        var result = controller.Result();

        // Assert
        var view = result.ShouldBeOfType<ViewResult>();
        var model = view.Model.ShouldBeOfType<OAuthResultViewModel>();
        model.Success.ShouldBeTrue();
        model.UserEmail.ShouldBe("user@example.com");
        model.RedirectUrl.ShouldBe("https://example.test/return");
        httpContext.Session.GetString(ReturnUrlKey).ShouldBeNull();
    }
}
