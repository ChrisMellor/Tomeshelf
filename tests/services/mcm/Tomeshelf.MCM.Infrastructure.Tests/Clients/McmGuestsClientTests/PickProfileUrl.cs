using Shouldly;
using Tomeshelf.MCM.Infrastructure.Clients;
using Tomeshelf.MCM.Infrastructure.Responses;

namespace Tomeshelf.MCM.Infrastructure.Tests.Clients.McmGuestsClientTests;

public class PickProfileUrl
{
    [Fact]
    public void ReturnsFirstAvailableProfileUrl()
    {
        var person = new McmEventResponse.Person
        {
            ProfileUrl = " ",
            Imdb = "https://imdb.example/guest",
            Twitter = "https://twitter.example/guest"
        };

        var url = McmGuestsClient.PickProfileUrl(person);

        url.ShouldBe("https://imdb.example/guest");
    }

    [Fact]
    public void ReturnsTrimmedProfileUrl_WhenPresent()
    {
        var person = new McmEventResponse.Person
        {
            ProfileUrl = "  https://profile.example/guest  ",
            Imdb = "https://imdb.example/guest"
        };

        var url = McmGuestsClient.PickProfileUrl(person);

        url.ShouldBe("https://profile.example/guest");
    }
}