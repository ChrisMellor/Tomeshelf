using Shouldly;
using Tomeshelf.MCM.Infrastructure.Responses;

namespace Tomeshelf.MCM.Infrastructure.Tests.McmEventResponseTests;

public class Image
{
    [Fact]
    public void CanSetAndGetValues()
    {
        // Arrange
        var big = "big.jpg";
        var med = "med.jpg";
        var small = "small.jpg";
        var thumb = "thumb.jpg";

        // Act
        var image = new McmEventResponse.Image
        {
            Big = big,
            Med = med,
            Small = small,
            Thumb = thumb
        };

        // Assert
        image.Big.ShouldBe(big);
        image.Med.ShouldBe(med);
        image.Small.ShouldBe(small);
        image.Thumb.ShouldBe(thumb);
    }
}