using Tomeshelf.MCM.Application.Services;

namespace Tomeshelf.MCM.Application.Tests.GuestsServiceTests;

public class SplitName
{
    [Theory]
    [InlineData("John Doe", "John", "Doe")]
    [InlineData("John", "John", "")]
    [InlineData("", "", "")]
    [InlineData(" ", "", "")]
    public void SplitsNameCorrectly(string name, string expectedFirstName, string expectedLastName)
    {
        // Arrange
        var input = name;

        // Act
        (string firstName, string lastName) = GuestsService.SplitName(input);

        // Assert
        firstName.ShouldBe(expectedFirstName);
        lastName.ShouldBe(expectedLastName);
    }
}
