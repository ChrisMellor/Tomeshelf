using Microsoft.AspNetCore.Http;
using Shouldly;
using Tomeshelf.Fitbit.Application;

namespace Tomeshelf.Fitbit.Infrastructure.Tests.FitbitAuthorizationServiceTests;

public class BuildCallbackUri
{
    [Fact]
    public void Throws_WhenBaseUriInvalid()
    {
        var options = new FitbitOptions { CallbackBaseUri = "not a uri" };
        var httpContext = new DefaultHttpContext();

        var exception = Should.Throw<InvalidOperationException>(() => FitbitAuthorizationService.BuildCallbackUri(options, httpContext.Request));

        exception.Message.ShouldBe("Invalid Fitbit CallbackBaseUri 'not a uri'.");
    }

    [Fact]
    public void Throws_WhenMissingBaseAndRequest()
    {
        var options = new FitbitOptions
        {
            CallbackBaseUri = null,
            CallbackPath = "/api/fitbit/auth/callback"
        };

        var exception = Should.Throw<InvalidOperationException>(() => FitbitAuthorizationService.BuildCallbackUri(options, null));

        exception.Message.ShouldBe("Fitbit CallbackBaseUri is not configured and no HTTP request is available to infer it.");
    }

    [Fact]
    public void UsesRequest_WhenBaseUriMissing()
    {
        var options = new FitbitOptions
        {
            CallbackBaseUri = null,
            CallbackPath = "callback"
        };
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Scheme = "https";
        httpContext.Request.Host = new HostString("example.test");
        httpContext.Request.PathBase = "/app";

        var result = FitbitAuthorizationService.BuildCallbackUri(options, httpContext.Request);

        result.ShouldBe(new Uri("https://example.test/app/callback"));
    }
}