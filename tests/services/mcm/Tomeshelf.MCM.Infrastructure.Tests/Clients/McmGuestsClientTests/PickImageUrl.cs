using Shouldly;
using Tomeshelf.MCM.Infrastructure.Clients;
using Tomeshelf.MCM.Infrastructure.Responses;

namespace Tomeshelf.MCM.Infrastructure.Tests.Clients.McmGuestsClientTests;

public class PickImageUrl
{
    /// <summary>
    ///     Returns the correct url.
    /// </summary>
    [Fact]
    public void ReturnsCorrectUrl()
    {
        // Arrange
        var images = new[]
        {
            new McmEventResponse.Image
            {
                Big = "big",
                Med = "med",
                Small = "small",
                Thumb = "thumb"
            },
            new McmEventResponse.Image { Med = "med2" }, new McmEventResponse.Image { Small = "small3" }, new McmEventResponse.Image { Thumb = "thumb4" }, new McmEventResponse.Image(), null
        };

        // Act
        var big = McmGuestsClient.PickImageUrl(new[] { images[0] });
        var med = McmGuestsClient.PickImageUrl(new[] { images[1] });
        var small = McmGuestsClient.PickImageUrl(new[] { images[2] });
        var thumb = McmGuestsClient.PickImageUrl(new[] { images[3] });
        var empty = McmGuestsClient.PickImageUrl(new[] { images[4] });
        var nullImage = McmGuestsClient.PickImageUrl(new[] { images[5]! });
        var none = McmGuestsClient.PickImageUrl(Array.Empty<McmEventResponse.Image>());
        var nullInput = McmGuestsClient.PickImageUrl(null);

        // Assert
        big.ShouldBe("big");
        med.ShouldBe("med2");
        small.ShouldBe("small3");
        thumb.ShouldBe("thumb4");
        empty.ShouldBe(string.Empty);
        nullImage.ShouldBe(string.Empty);
        none.ShouldBe(string.Empty);
        nullInput.ShouldBe(string.Empty);
    }
}