using Shouldly;
using Tomeshelf.MCM.Infrastructure.Clients;

namespace Tomeshelf.MCM.Infrastructure.Tests.Clients.McmGuestsClientTests;

public class FirstNonEmpty
{
    [Fact]
    public void ReturnsEmpty_WhenAllValuesMissing()
    {
        string? first = null;
        var second = "  ";

        var result = McmGuestsClient.FirstNonEmpty(first, second);

        result.ShouldBe(string.Empty);
    }

    [Fact]
    public void ReturnsFirstNonEmptyValue()
    {
        var first = " ";
        var second = "  value  ";
        var third = "other";

        var result = McmGuestsClient.FirstNonEmpty(first, second, third);

        result.ShouldBe("value");
    }
}