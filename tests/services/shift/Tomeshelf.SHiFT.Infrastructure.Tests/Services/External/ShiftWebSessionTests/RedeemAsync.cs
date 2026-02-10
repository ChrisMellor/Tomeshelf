using Shouldly;
using Tomeshelf.SHiFT.Infrastructure.Tests.TestUtilities;

namespace Tomeshelf.SHiFT.Infrastructure.Tests.Services.External.ShiftWebSessionTests;

public class RedeemAsync
{
    /// <summary>
    ///     Posts the to redeem endpoint.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task PostsToRedeemEndpoint()
    {
        // Arrange
        var handler = new ShiftWebSessionTestHarness.RoutingHandler();
        handler.Responses["/code_redemptions"] = string.Empty;
        await using var session = ShiftWebSessionTestHarness.CreateSession(handler);

        // Act
        await session.RedeemAsync("code=123", CancellationToken.None);

        // Assert
        handler.LastRequest.ShouldNotBeNull();
        handler.LastRequest!.Method.ShouldBe(HttpMethod.Post);
        handler.LastRequest.RequestUri.ShouldBe(new Uri("https://shift.test/code_redemptions"));
        handler.LastRequestContentType.ShouldBe("application/x-www-form-urlencoded");
        handler.LastRequestBody.ShouldBe("code=123");
    }
}