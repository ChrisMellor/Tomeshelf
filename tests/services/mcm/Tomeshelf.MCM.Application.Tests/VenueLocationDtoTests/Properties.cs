using Shouldly;
using Tomeshelf.MCM.Application.Mcm;

namespace Tomeshelf.MCM.Application.Tests.VenueLocationDtoTests;

public class Properties
{
    [Fact]
    public void CanSetAndGetValues()
    {
        var id = "venue-id-1";
        var name = "Convention Center";

        var dto = new VenueLocationDto
        {
            Id = id,
            Name = name
        };

        dto.Id.ShouldBe(id);
        dto.Name.ShouldBe(name);
    }

    [Fact]
    public void DefaultsAreNull()
    {
        var dto = new VenueLocationDto();

        var id = dto.Id;
        var name = dto.Name;

        id.ShouldBeNull();
        name.ShouldBeNull();
    }
}