using Shouldly;
using Tomeshelf.MCM.Infrastructure.Clients;
using Tomeshelf.MCM.Infrastructure.Responses;

namespace Tomeshelf.MCM.Infrastructure.Tests.Clients.McmGuestsClientTests;

public class MapGuests
{
    [Fact]
    public void MapsGuestsCorrectly()
    {
        var people = new[]
        {
            new McmEventResponse.Person
            {
                FirstName = "John",
                LastName = "Doe"
            },
            null, new McmEventResponse.Person
            {
                FirstName = "Jane",
                LastName = "Doe",
                Bio = "Bio",
                KnownFor = "Known"
            },
            new McmEventResponse.Person
            {
                FirstName = null,
                LastName = null,
                AltName = ""
            }
        };

        var result = McmGuestsClient.MapGuests(people);

        result.Count.ShouldBe(2);
        result[0]
           .Name
           .ShouldBe("John Doe");
        result[1]
           .Name
           .ShouldBe("Jane Doe");
        result[1]
           .Description
           .ShouldBe("Bio");
    }

    [Fact]
    public void ReturnsEmptyList_WhenPeopleEmpty()
    {
        var people = Array.Empty<McmEventResponse.Person?>();

        var result = McmGuestsClient.MapGuests(people);

        result.ShouldBeEmpty();
    }
}