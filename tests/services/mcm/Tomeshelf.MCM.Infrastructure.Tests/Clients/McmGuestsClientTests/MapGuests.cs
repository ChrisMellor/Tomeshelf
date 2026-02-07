using Tomeshelf.MCM.Infrastructure.Clients;
using Tomeshelf.MCM.Infrastructure.Responses;

namespace Tomeshelf.MCM.Infrastructure.Tests.Clients.McmGuestsClientTests;

public class MapGuests
{
    [Fact]
    public void MapsGuestsCorrectly()
    {
        // Arrange
        var people = new McmEventResponse.Person?[]
        {
            new McmEventResponse.Person { FirstName = "John", LastName = "Doe" },
            null,
            new McmEventResponse.Person { FirstName = "Jane", LastName = "Doe", Bio = "Bio", KnownFor = "Known" },
            new McmEventResponse.Person { FirstName = null, LastName = null, AltName = "" }
        };

        // Act
        var result = McmGuestsClient.MapGuests(people);

        // Assert
        result.Count.ShouldBe(2);
        result[0].Name.ShouldBe("John Doe");
        result[1].Name.ShouldBe("Jane Doe");
        result[1].Description.ShouldBe("Bio");
    }

    [Fact]
    public void ReturnsEmptyList_WhenPeopleEmpty()
    {
        // Arrange
        var people = Array.Empty<McmEventResponse.Person?>();

        // Act
        var result = McmGuestsClient.MapGuests(people);

        // Assert
        result.ShouldBeEmpty();
    }
}
