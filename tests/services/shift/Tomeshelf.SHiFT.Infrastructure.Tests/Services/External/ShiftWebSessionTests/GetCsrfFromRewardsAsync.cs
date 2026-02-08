using Shouldly;
using Tomeshelf.SHiFT.Infrastructure.Tests.TestUtilities;

namespace Tomeshelf.SHiFT.Infrastructure.Tests.Services.External.ShiftWebSessionTests;

public class GetCsrfFromRewardsAsync
{
    [Fact]
    public async Task ReturnsToken()
    {
        // Arrange
        var handler = new ShiftWebSessionTestHarness.RoutingHandler();
        handler.Responses["/rewards"] = "<meta name=\"csrf-token\" content=\"token-789\">";
        await using var session = ShiftWebSessionTestHarness.CreateSession(handler);

        // Act
        var token = await session.GetCsrfFromRewardsAsync("csrf", "user@example.com", "password", CancellationToken.None);

        // Assert
        token.ShouldBe("token-789");
    }

    [Fact]
    public async Task ThrowsWhenMissingToken()
    {
        // Arrange
        var handler = new ShiftWebSessionTestHarness.RoutingHandler();
        handler.Responses["/rewards"] = "<html><head></head><body>No token</body></html>";
        await using var session = ShiftWebSessionTestHarness.CreateSession(handler);

        var action = () => session.GetCsrfFromRewardsAsync("csrf", "user@example.com", "password", CancellationToken.None);

        // Act
        var exception = await Should.ThrowAsync<InvalidOperationException>(action);
        // Assert
        exception.Message.ShouldBe("CSRF token not found on /rewards.");
    }
}