using Tomeshelf.SHiFT.Application.Features.KeyDiscovery;

namespace Tomeshelf.SHiFT.Application.Tests.Features.KeyDiscovery.ShiftKeyMatcherTests;

public class Extract
{
    [Fact]
    public void FindsKeysAndNormalizesCase()
    {
        // Arrange
        var text = "Some codes: abcde-fghij-klmno-pqrst-uvwxy and 12345-ABCDE-67890-FGHIJ-KLMNO.";

        // Act
        var results = ShiftKeyMatcher.Extract(text);

        // Assert
        results.Count.ShouldBe(2);
        results[0].ShouldBe("ABCDE-FGHIJ-KLMNO-PQRST-UVWXY");
        results[1].ShouldBe("12345-ABCDE-67890-FGHIJ-KLMNO");
    }

    [Fact]
    public void WhenNullOrWhitespace_ReturnsEmpty()
    {
        // Arrange
        string? nullValue = null;
        var whitespace = "  ";

        // Act
        var nullResult = ShiftKeyMatcher.Extract(nullValue);
        var whitespaceResult = ShiftKeyMatcher.Extract(whitespace);

        // Assert
        nullResult.ShouldBeEmpty();
        whitespaceResult.ShouldBeEmpty();
    }

    [Fact]
    public void WhenNoMatches_ReturnsEmpty()
    {
        // Arrange
        var text = "No valid codes here, just ABCDE-FGHIJ-KLMNO.";

        // Act
        var results = ShiftKeyMatcher.Extract(text);

        // Assert
        results.ShouldBeEmpty();
    }
}
