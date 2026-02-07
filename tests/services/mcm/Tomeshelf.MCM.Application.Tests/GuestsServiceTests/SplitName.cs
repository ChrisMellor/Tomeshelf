using Shouldly;
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
        var input = name;

        var (firstName, lastName) = GuestsService.SplitName(input);

        firstName.ShouldBe(expectedFirstName);
        lastName.ShouldBe(expectedLastName);
    }
}