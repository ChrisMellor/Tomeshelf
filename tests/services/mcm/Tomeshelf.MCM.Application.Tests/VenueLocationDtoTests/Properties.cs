using Shouldly;
using Tomeshelf.MCM.Application.Mcm;

namespace Tomeshelf.MCM.Application.Tests.VenueLocationDtoTests;

public class Properties
{
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

    [Fact]
    public void DefaultsAreNull()
    {
        // Arrange
        var dto = new VenueLocationDto();

        var id = dto.Id;
        // Act
        var name = dto.Name;

        // Assert
        id.ShouldBeNull();
        name.ShouldBeNull();
    }
}