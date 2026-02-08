using Shouldly;
using Tomeshelf.SHiFT.Infrastructure.Tests.TestUtilities;

namespace Tomeshelf.SHiFT.Infrastructure.Tests.Services.External.ShiftWebSessionTests;

public class GetCsrfFromHomeAsync
{
    [Fact]
    public async Task ReturnsToken()
    {
        // Arrange
        var handler = new ShiftWebSessionTestHarness.RoutingHandler();
        handler.Responses["/home"] = "<meta name=\"csrf-token\" content=\"token-123\">";
        await using var session = ShiftWebSessionTestHarness.CreateSession(handler);

        // Act
        var token = await session.GetCsrfFromHomeAsync(CancellationToken.None);

        // Assert
        token.ShouldBe("token-123");
    }

    [Fact]
    public async Task ThrowsWhenMissingToken()
    {
        // Arrange
        var handler = new ShiftWebSessionTestHarness.RoutingHandler();
        handler.Responses["/home"] = "<html><head></head><body>No token</body></html>";
        await using var session = ShiftWebSessionTestHarness.CreateSession(handler);

        // Act
        var action = () => session.GetCsrfFromHomeAsync(CancellationToken.None);

        // Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(action);
        exception.Message.ShouldBe("CSRF token not found on /home.");
    }
}