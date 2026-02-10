using Shouldly;
using Tomeshelf.MCM.Application.Records;
using Tomeshelf.MCM.Application.Services;

namespace Tomeshelf.MCM.Application.Tests.GuestsServiceTests;

public class MapRecordToGuest
{
    /// <summary>
    ///     Maps null socials when the profile URL is empty.
    /// </summary>
    [Fact]
    public void MapsNullSocials_WhenProfileUrlEmpty()
    {
        // Arrange
        var guestRecord = new GuestRecord("Test Guest", "Test Description", "", "http://example.com/image.jpg");

        // Act
        var result = GuestsService.MapRecordToGuest(guestRecord);

        // Assert
        result.ShouldNotBeNull();
        result.Information.ShouldNotBeNull();
        result.Information!.Socials.ShouldBeNull();
    }

    /// <summary>
    ///     Maps the record correctly.
    /// </summary>
    [Fact]
    public void MapsRecordCorrectly()
    {
        // Arrange
        var guestRecord = new GuestRecord("Test Guest", "Test Description", "http://example.com/profile", "http://example.com/image.jpg");

        // Act
        var result = GuestsService.MapRecordToGuest(guestRecord);

        // Assert
        result.ShouldNotBeNull();
        result.Information.ShouldNotBeNull();
        result.Information!.FirstName.ShouldBe("Test");
        result.Information.LastName.ShouldBe("Guest");
        result.Information.Bio.ShouldBe("Test Description");
        result.Information.ImageUrl.ShouldBe("http://example.com/image.jpg");
        result.Information.Socials.ShouldNotBeNull();
        result.Information.Socials!.Imdb.ShouldBe("http://example.com/profile");
    }
}