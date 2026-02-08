using Microsoft.AspNetCore.Http;
using Shouldly;
using Tomeshelf.Fitbit.Application;

namespace Tomeshelf.Fitbit.Infrastructure.Tests.FitbitAuthorizationServiceTests;

public class BuildCallbackUri
{
    [Fact]
    public void Throws_WhenBaseUriInvalid()
    {
        // Arrange
        var options = new FitbitOptions { CallbackBaseUri = "not a uri" };
        var httpContext = new DefaultHttpContext();

        // Act
        var exception = Should.Throw<InvalidOperationException>(() => FitbitAuthorizationService.BuildCallbackUri(options, httpContext.Request));

        // Assert
        exception.Message.ShouldBe("Invalid Fitbit CallbackBaseUri 'not a uri'.");
    }

    [Fact]
    public void Throws_WhenMissingBaseAndRequest()
    {
        // Arrange
        var options = new FitbitOptions
        {
            CallbackBaseUri = null,
            CallbackPath = "/api/fitbit/auth/callback"
        };

        // Act
        var exception = Should.Throw<InvalidOperationException>(() => FitbitAuthorizationService.BuildCallbackUri(options, null));

        // Assert
        exception.Message.ShouldBe("Fitbit CallbackBaseUri is not configured and no HTTP request is available to infer it.");
    }

    [Fact]
    public void UsesRequest_WhenBaseUriMissing()
    {
        // Arrange
        var options = new FitbitOptions
        {
            CallbackBaseUri = null,
            CallbackPath = "callback"
        };
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Scheme = "https";
        httpContext.Request.Host = new HostString("example.test");
        httpContext.Request.PathBase = "/app";

        // Act
        var result = FitbitAuthorizationService.BuildCallbackUri(options, httpContext.Request);

        // Assert
        result.ShouldBe(new Uri("https://example.test/app/callback"));
    }
}