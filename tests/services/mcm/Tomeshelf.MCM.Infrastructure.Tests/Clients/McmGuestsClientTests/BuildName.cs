using Shouldly;
using Tomeshelf.MCM.Infrastructure.Clients;
using Tomeshelf.MCM.Infrastructure.Responses;

namespace Tomeshelf.MCM.Infrastructure.Tests.Clients.McmGuestsClientTests;

public class BuildName
{
    [Theory]
    [InlineData("John", "Doe", "John Doe")]
    [InlineData("John", null, "John")]
    [InlineData(null, "Doe", "Doe")]
    [InlineData(null, null, "Johnny")]
    [InlineData(" ", " ", "Johnny")]
    public void BuildsNameCorrectly(string? firstName, string? lastName, string expectedName)
    {
        var person = new McmEventResponse.Person
        {
            FirstName = firstName,
            LastName = lastName,
            AltName = "Johnny"
        };

        var result = McmGuestsClient.BuildName(person);

        result.ShouldBe(expectedName);
    }
}