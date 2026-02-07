using Shouldly;
using Tomeshelf.MCM.Application.Mcm;

namespace Tomeshelf.MCM.Application.Tests.ImageSetDtoTests;

public class Properties
{
    [Fact]
    public void CanSetAndGetValues()
    {
        var big = "big-url";
        var med = "med-url";
        var small = "small-url";
        var thumb = "thumb-url";

        var dto = new ImageSetDto
        {
            Big = big,
            Med = med,
            Small = small,
            Thumb = thumb
        };

        dto.Big.ShouldBe(big);
        dto.Med.ShouldBe(med);
        dto.Small.ShouldBe(small);
        dto.Thumb.ShouldBe(thumb);
    }

    [Fact]
    public void DefaultsAreNull()
    {
        var dto = new ImageSetDto();

        var big = dto.Big;
        var med = dto.Med;
        var small = dto.Small;
        var thumb = dto.Thumb;

        big.ShouldBeNull();
        med.ShouldBeNull();
        small.ShouldBeNull();
        thumb.ShouldBeNull();
    }
}