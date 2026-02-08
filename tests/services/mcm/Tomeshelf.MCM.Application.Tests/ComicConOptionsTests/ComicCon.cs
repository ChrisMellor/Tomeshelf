using Shouldly;

namespace Tomeshelf.MCM.Application.Tests.ComicConOptionsTests;

public class ComicCon
{
    [Fact]
    public void CanSetAndGetList()
    {
        // Arrange
        var options = new ComicConOptions();
        // Act
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

        // Assert
        options.ComicCon = locations;

        options.ComicCon.ShouldBeSameAs(locations);
    }

    [Fact]
    public void DefaultsToEmptyList()
    {
        // Arrange
        var options = new ComicConOptions();

        // Act
        var comicCon = options.ComicCon;

        // Assert
        comicCon.ShouldNotBeNull();
        comicCon.ShouldBeEmpty();
    }
}