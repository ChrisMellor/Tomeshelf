using Shouldly;
using Tomeshelf.MCM.Application.Services;

namespace Tomeshelf.MCM.Application.Tests.GuestsServiceTests;

public class SplitName
{
    /// <summary>
    ///     Splits the name correctly.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="expectedFirstName">The expected first name.</param>
    /// <param name="expectedLastName">The expected last name.</param>
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
        var (firstName, lastName) = GuestsService.SplitName(input);

        // Assert
        firstName.ShouldBe(expectedFirstName);
        lastName.ShouldBe(expectedLastName);
    }
}