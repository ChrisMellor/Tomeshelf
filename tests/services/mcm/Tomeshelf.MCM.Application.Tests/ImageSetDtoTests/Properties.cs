using Shouldly;
using Tomeshelf.MCM.Application.Mcm;

namespace Tomeshelf.MCM.Application.Tests.ImageSetDtoTests;

public class Properties
{
    /// <summary>
    ///     Determines whether the current instance can set and get values.
    /// </summary>
    [Fact]
    public void CanSetAndGetValues()
    {
        // Arrange
        var big = "big-url";
        var med = "med-url";
        var small = "small-url";
        var thumb = "thumb-url";

        // Act
        var dto = new ImageSetDto
        {
            Big = big,
            Med = med,
            Small = small,
            Thumb = thumb
        };

        // Assert
        dto.Big.ShouldBe(big);
        dto.Med.ShouldBe(med);
        dto.Small.ShouldBe(small);
        dto.Thumb.ShouldBe(thumb);
    }

    /// <summary>
    ///     Defaults the are null.
    /// </summary>
    [Fact]
    public void DefaultsAreNull()
    {
        // Arrange
        var dto = new ImageSetDto();

        // Act
        var big = dto.Big;
        var med = dto.Med;
        var small = dto.Small;
        var thumb = dto.Thumb;

        // Assert
        big.ShouldBeNull();
        med.ShouldBeNull();
        small.ShouldBeNull();
        thumb.ShouldBeNull();
    }
}