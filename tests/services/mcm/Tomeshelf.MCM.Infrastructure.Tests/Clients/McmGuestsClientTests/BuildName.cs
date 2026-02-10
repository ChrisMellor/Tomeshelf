using Shouldly;
using Tomeshelf.MCM.Infrastructure.Clients;
using Tomeshelf.MCM.Infrastructure.Responses;

namespace Tomeshelf.MCM.Infrastructure.Tests.Clients.McmGuestsClientTests;

public class BuildName
{
    /// <summary>
    ///     Builds the name correctly.
    /// </summary>
    /// <param name="firstName">The first name.</param>
    /// <param name="lastName">The last name.</param>
    /// <param name="expectedName">The expected name.</param>
    [Theory]
    [InlineData("John", "Doe", "John Doe")]
    [InlineData("John", null, "John")]
    [InlineData(null, "Doe", "Doe")]
    [InlineData(null, null, "Johnny")]
    [InlineData(" ", " ", "Johnny")]
    public void BuildsNameCorrectly(string? firstName, string? lastName, string expectedName)
    {
        // Arrange
        var person = new McmEventResponse.Person
        {
            FirstName = firstName,
            LastName = lastName,
            AltName = "Johnny"
        };

        // Act
        var result = McmGuestsClient.BuildName(person);

        // Assert
        result.ShouldBe(expectedName);
    }
}