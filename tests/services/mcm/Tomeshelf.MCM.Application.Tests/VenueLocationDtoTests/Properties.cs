using Shouldly;
using Tomeshelf.MCM.Application.Mcm;

namespace Tomeshelf.MCM.Application.Tests.VenueLocationDtoTests;

public class Properties
{
    /// <summary>
    ///     Determines whether the current instance can set and get values.
    /// </summary>
    [Fact]
    public void CanSetAndGetValues()
    {
        // Arrange
        var id = "venue-id-1";
        var name = "Convention Center";

        // Act
        var dto = new VenueLocationDto
        {
            Id = id,
            Name = name
        };

        // Assert
        dto.Id.ShouldBe(id);
        dto.Name.ShouldBe(name);
    }

    /// <summary>
    ///     Defaults the are null.
    /// </summary>
    [Fact]
    public void DefaultsAreNull()
    {
        // Arrange
        var dto = new VenueLocationDto();

        // Act
        var id = dto.Id;
        var name = dto.Name;

        // Assert
        id.ShouldBeNull();
        name.ShouldBeNull();
    }
}