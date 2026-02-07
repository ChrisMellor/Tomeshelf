using Shouldly;
using Tomeshelf.SHiFT.Application.Features.KeyDiscovery;

namespace Tomeshelf.SHiFT.Application.Tests.Features.KeyDiscovery.ShiftKeyMatcherTests;

public class Extract
{
    [Fact]
    public void FindsKeysAndNormalizesCase()
    {
        var text = "Some codes: abcde-fghij-klmno-pqrst-uvwxy and 12345-ABCDE-67890-FGHIJ-KLMNO.";

        var results = ShiftKeyMatcher.Extract(text);

        results.Count.ShouldBe(2);
        results[0]
           .ShouldBe("ABCDE-FGHIJ-KLMNO-PQRST-UVWXY");
        results[1]
           .ShouldBe("12345-ABCDE-67890-FGHIJ-KLMNO");
    }

    [Fact]
    public void WhenNoMatches_ReturnsEmpty()
    {
        var text = "No valid codes here, just ABCDE-FGHIJ-KLMNO.";

        var results = ShiftKeyMatcher.Extract(text);

        results.ShouldBeEmpty();
    }

    [Fact]
    public void WhenNullOrWhitespace_ReturnsEmpty()
    {
        string? nullValue = null;
        var whitespace = "  ";

        var nullResult = ShiftKeyMatcher.Extract(nullValue);
        var whitespaceResult = ShiftKeyMatcher.Extract(whitespace);

        nullResult.ShouldBeEmpty();
        whitespaceResult.ShouldBeEmpty();
    }
}