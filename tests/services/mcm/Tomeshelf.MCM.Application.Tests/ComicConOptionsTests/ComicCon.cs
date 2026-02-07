using Shouldly;

namespace Tomeshelf.MCM.Application.Tests.ComicConOptionsTests;

public class ComicCon
{
    [Fact]
    public void CanSetAndGetList()
    {
        var options = new ComicConOptions();
        var locations = new List<Location>
        {
            new Location
            {
                City = "London",
                Key = Guid.NewGuid()
            },
            new Location
            {
                City = "New York",
                Key = Guid.NewGuid()
            }
        };

        options.ComicCon = locations;

        options.ComicCon.ShouldBeSameAs(locations);
    }

    [Fact]
    public void DefaultsToEmptyList()
    {
        var options = new ComicConOptions();

        var comicCon = options.ComicCon;

        comicCon.ShouldNotBeNull();
        comicCon.ShouldBeEmpty();
    }
}