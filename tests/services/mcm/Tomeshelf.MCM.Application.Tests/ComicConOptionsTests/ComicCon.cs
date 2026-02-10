using Shouldly;

namespace Tomeshelf.MCM.Application.Tests.ComicConOptionsTests;

public class ComicCon
{
    /// <summary>
    ///     Determines whether the current instance can set and get list.
    /// </summary>
    [Fact]
    public void CanSetAndGetList()
    {
        // Arrange
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

        // Act
        options.ComicCon = locations;

        // Assert
        options.ComicCon.ShouldBeSameAs(locations);
    }

    /// <summary>
    ///     Defaults the to empty list.
    /// </summary>
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