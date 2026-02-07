using Shouldly;
using Tomeshelf.SHiFT.Infrastructure.Tests.TestUtilities;

namespace Tomeshelf.SHiFT.Infrastructure.Tests.Services.External.ShiftWebSessionTests;

public class RedeemAsync
{
    [Fact]
    public async Task PostsToRedeemEndpoint()
    {
        var handler = new ShiftWebSessionTestHarness.RoutingHandler();
        handler.Responses["/code_redemptions"] = string.Empty;
        await using var session = ShiftWebSessionTestHarness.CreateSession(handler);

        await session.RedeemAsync("code=123", CancellationToken.None);

        handler.LastRequest.ShouldNotBeNull();
        handler.LastRequest!.Method.ShouldBe(HttpMethod.Post);
        handler.LastRequest.RequestUri.ShouldBe(new Uri("https://shift.test/code_redemptions"));
        handler.LastRequestContentType.ShouldBe("application/x-www-form-urlencoded");
        handler.LastRequestBody.ShouldBe("code=123");
    }
}