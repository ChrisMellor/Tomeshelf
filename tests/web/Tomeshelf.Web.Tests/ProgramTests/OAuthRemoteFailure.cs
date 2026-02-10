using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Shouldly;
using Tomeshelf.Web.Tests.TestUtilities;

namespace Tomeshelf.Web.Tests.ProgramTests;

public class OAuthRemoteFailure
{
    /// <summary>
    ///     Writes the session and redirects.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task WritesSessionAndRedirects()
    {
        // Arrange
        using var app = ProgramTestHarness.BuildApp(Environments.Development, ProgramTestHarness.GoogleDriveConfig("config@example.test"));
        var options = ProgramTestHarness.GetOAuthOptions(app);
        var (httpContext, _) = ProgramTestHarness.CreateSessionContext();
        var context = ProgramTestHarness.CreateRemoteFailureContext(httpContext, options, new InvalidOperationException("boom"));

        // Act
        await options.Events.RemoteFailure(context);

        // Assert
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