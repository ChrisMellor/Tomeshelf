using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Shouldly;
using Tomeshelf.Web.Tests.TestUtilities;

namespace Tomeshelf.Web.Tests.ProgramTests;

public class OAuthRemoteFailure
{
    [Fact]
    public async Task WritesSessionAndRedirects()
    {
        using var app = ProgramTestHarness.BuildApp(Environments.Development, ProgramTestHarness.GoogleDriveConfig("config@example.test"));
        var options = ProgramTestHarness.GetOAuthOptions(app);
        var (httpContext, _) = ProgramTestHarness.CreateSessionContext();
        var context = ProgramTestHarness.CreateRemoteFailureContext(httpContext, options, new InvalidOperationException("boom"));

        await options.Events.RemoteFailure(context);

        httpContext.Session
                   .GetString(ProgramTestHarness.ErrorKey)
                   .ShouldBe("boom");
        httpContext.Response.StatusCode.ShouldBe(StatusCodes.Status302Found);
        httpContext.Response
                   .Headers
                   .Location
                   .ToString()
                   .ShouldBe("/drive-auth/result");
    }
}