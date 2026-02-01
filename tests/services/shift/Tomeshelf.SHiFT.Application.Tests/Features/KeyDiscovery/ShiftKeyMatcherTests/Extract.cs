using FluentAssertions;
using Tomeshelf.SHiFT.Application.Features.KeyDiscovery;

namespace Tomeshelf.SHiFT.Application.Tests.Features.KeyDiscovery.ShiftKeyMatcherTests;

public class Extract
{
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
        nullResult.Should().BeEmpty();
        whitespaceResult.Should().BeEmpty();
    }

    [Fact]
    public void FindsKeysAndNormalizesCase()
    {
        // Arrange
        var text = "Some codes: abcde-fghij-klmno-pqrst-uvwxy and 12345-ABCDE-67890-FGHIJ-KLMNO.";

        // Act
        var results = ShiftKeyMatcher.Extract(text);

        // Assert
        results.Should().HaveCount(2);
        results[0].Should().Be("ABCDE-FGHIJ-KLMNO-PQRST-UVWXY");
        results[1].Should().Be("12345-ABCDE-67890-FGHIJ-KLMNO");
    }
}
