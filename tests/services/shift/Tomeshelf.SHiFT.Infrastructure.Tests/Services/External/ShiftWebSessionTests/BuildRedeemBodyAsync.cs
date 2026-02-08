using Shouldly;
using Tomeshelf.SHiFT.Infrastructure.Tests.TestUtilities;

namespace Tomeshelf.SHiFT.Infrastructure.Tests.Services.External.ShiftWebSessionTests;

public class BuildRedeemBodyAsync
{
    [Fact]
    public async Task ReturnsMatchingOptions()
    {
        // Arrange
        var handler = new ShiftWebSessionTestHarness.RoutingHandler();
        handler.Responses["/entitlement_offer_codes"] = """
                                                        <form>
                                                          <input name="archway_code_redemption[service]" value="psn" />
                                                          <input name="archway_code_redemption[title]" value="Golden Keys" />
                                                          <input name="archway_code_redemption[code]" value="CODE-123" />
                                                          <button>PlayStation</button>
                                                        </form>
                                                        <form>
                                                          <input name="archway_code_redemption[service]" value="steam" />
                                                        </form>
                                                        """;
        await using var session = ShiftWebSessionTestHarness.CreateSession(handler);

        // Act
        var options = await session.BuildRedeemBodyAsync("CODE-123", "csrf", "psn", CancellationToken.None);

        // Assert
        options.ShouldHaveSingleItem();
        options[0]
           .Service
           .ShouldBe("psn");
        options[0]
           .Title
           .ShouldBe("Golden Keys");
        options[0]
           .DisplayName
           .ShouldBe("PlayStation");
        options[0]
           .FormBody
           .ShouldContain("archway_code_redemption%5Bservice%5D=psn");
        options[0]
           .FormBody
           .ShouldContain("archway_code_redemption%5Btitle%5D=Golden+Keys");
        options[0]
           .FormBody
           .ShouldContain("archway_code_redemption%5Bcode%5D=CODE-123");
        options[0]
           .FormBody
           .ShouldContain("utf8=%E2%9C%93");
    }

    [Fact]
    public async Task ReturnsNullDisplayName_WhenMissingButton()
    {
        // Arrange
        var handler = new ShiftWebSessionTestHarness.RoutingHandler();
        handler.Responses["/entitlement_offer_codes"] = """
                                                        <form>
                                                          <input name="archway_code_redemption[service]" value="psn" />
                                                          <input name="archway_code_redemption[title]" value="Golden Keys" />
                                                          <input name="archway_code_redemption[code]" value="CODE-123" />
                                                        </form>
                                                        """;
        await using var session = ShiftWebSessionTestHarness.CreateSession(handler);

        // Act
        var options = await session.BuildRedeemBodyAsync("CODE-123", "csrf", "psn", CancellationToken.None);

        // Assert
        options.ShouldHaveSingleItem();
        options[0]
           .DisplayName
           .ShouldBeNull();
    }

    [Fact]
    public async Task ThrowsWhenNoMatchingForm()
    {
        // Arrange
        var handler = new ShiftWebSessionTestHarness.RoutingHandler();
        handler.Responses["/entitlement_offer_codes"] = "<form></form>";
        await using var session = ShiftWebSessionTestHarness.CreateSession(handler);

        var action = () => session.BuildRedeemBodyAsync("CODE-123", "csrf", "xbox", CancellationToken.None);

        // Act
        var exception = await Should.ThrowAsync<InvalidOperationException>(action);
        // Assert
        exception.Message.ShouldBe("No redemption form found for service 'xbox'.");
    }
}