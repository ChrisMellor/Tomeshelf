using Shouldly;
using Tomeshelf.SHiFT.Infrastructure.Tests.TestUtilities;

namespace Tomeshelf.SHiFT.Infrastructure.Tests.Services.External.ShiftWebSessionTests;

public class GetCsrfFromHomeAsync
{
    [Fact]
    public async Task ReturnsToken()
    {
        var handler = new ShiftWebSessionTestHarness.RoutingHandler();
        handler.Responses["/home"] = "<meta name=\"csrf-token\" content=\"token-123\">";
        await using var session = ShiftWebSessionTestHarness.CreateSession(handler);

        var token = await session.GetCsrfFromHomeAsync(CancellationToken.None);

        token.ShouldBe("token-123");
    }

    [Fact]
    public async Task ThrowsWhenMissingToken()
    {
        var handler = new ShiftWebSessionTestHarness.RoutingHandler();
        handler.Responses["/home"] = "<html><head></head><body>No token</body></html>";
        await using var session = ShiftWebSessionTestHarness.CreateSession(handler);

        var action = () => session.GetCsrfFromHomeAsync(CancellationToken.None);

        var exception = await Should.ThrowAsync<InvalidOperationException>(action);
        exception.Message.ShouldBe("CSRF token not found on /home.");
    }
}