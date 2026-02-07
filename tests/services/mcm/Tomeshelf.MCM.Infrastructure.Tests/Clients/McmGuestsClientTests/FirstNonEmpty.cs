using Tomeshelf.MCM.Infrastructure.Clients;

namespace Tomeshelf.MCM.Infrastructure.Tests.Clients.McmGuestsClientTests;

public class FirstNonEmpty
{
    [Fact]
    public void ReturnsFirstNonEmptyValue()
    {
        // Arrange
        var first = " ";
        var second = "  value  ";
        var third = "other";

        // Act
        var result = McmGuestsClient.FirstNonEmpty(first, second, third);

        // Assert
        result.ShouldBe("value");
    }

    [Fact]
    public void ReturnsEmpty_WhenAllValuesMissing()
    {
        // Arrange
        string? first = null;
        var second = "  ";

        // Act
        var result = McmGuestsClient.FirstNonEmpty(first, second);

        // Assert
        result.ShouldBe(string.Empty);
    }
}
