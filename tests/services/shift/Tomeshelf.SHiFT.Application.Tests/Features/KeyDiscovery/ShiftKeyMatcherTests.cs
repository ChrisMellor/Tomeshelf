using Tomeshelf.SHiFT.Application.Features.KeyDiscovery;
using Xunit;

namespace Tomeshelf.SHiFT.Application.Tests.Features.KeyDiscovery;

public class ShiftKeyMatcherTests
{
    [Fact]
    public void Extract_WhenNullOrWhitespace_ReturnsEmpty()
    {
        Assert.Empty(ShiftKeyMatcher.Extract(null));
        Assert.Empty(ShiftKeyMatcher.Extract("  "));
    }

    [Fact]
    public void Extract_FindsKeysAndNormalizesCase()
    {
        // Arrange
        var text = "Some codes: abcde-fghij-klmno-pqrst-uvwxy and 12345-ABCDE-67890-FGHIJ-KLMNO.";

        // Act
        var results = ShiftKeyMatcher.Extract(text);

        // Assert
        Assert.Equal(2, results.Count);
        Assert.Equal("ABCDE-FGHIJ-KLMNO-PQRST-UVWXY", results[0]);
        Assert.Equal("12345-ABCDE-67890-FGHIJ-KLMNO", results[1]);
    }
}
