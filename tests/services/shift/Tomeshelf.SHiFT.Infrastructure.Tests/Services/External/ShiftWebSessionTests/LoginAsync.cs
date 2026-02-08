using Shouldly;
using Tomeshelf.SHiFT.Infrastructure.Tests.TestUtilities;

namespace Tomeshelf.SHiFT.Infrastructure.Tests.Services.External.ShiftWebSessionTests;

public class LoginAsync
{
    [Fact]
    public async Task PostsToSessionsEndpoint()
    {
        // Arrange
        var handler = new ShiftWebSessionTestHarness.RoutingHandler();
        handler.Responses["/sessions"] = "<html>ok</html>";
        await using var session = ShiftWebSessionTestHarness.CreateSession(handler);

        // Act
        await session.LoginAsync("user@example.com", "secret", "csrf", CancellationToken.None);

        // Assert
        handler.LastRequest.ShouldNotBeNull();
        handler.LastRequest!.Method.ShouldBe(HttpMethod.Post);
        handler.LastRequest.RequestUri.ShouldBe(new Uri("https://shift.test/sessions"));
        handler.LastRequest.Headers.Referrer.ShouldBe(new Uri("https://shift.gearboxsoftware.com/home"));
        handler.LastRequestContentType.ShouldBe("application/x-www-form-urlencoded");
        handler.LastRequestBody.ShouldBe("authenticity_token=csrf&user%5Bemail%5D=user%40example.com&user%5Bpassword%5D=secret");
    }
}