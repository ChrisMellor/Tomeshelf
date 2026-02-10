using Shouldly;
using Tomeshelf.HumbleBundle.Api.Controllers;
using Tomeshelf.HumbleBundle.Api.Tests.TestUtilities;

namespace Tomeshelf.HumbleBundle.Api.Tests.Controllers.BundleResponseTests;

public class FromDto
{
    /// <summary>
    ///     Returns null seconds when the already is expired.
    /// </summary>
    [Fact]
    public void ReturnsNullSeconds_WhenAlreadyExpired()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var dto = BundlesControllerTestHarness.CreateDto(now.AddMinutes(-5));

        // Act
        var response = BundlesController.BundleResponse.FromDto(dto, now);

        // Assert
        response.SecondsRemaining.ShouldBeNull();
    }

    /// <summary>
    ///     Returns null seconds when there is no end date.
    /// </summary>
    [Fact]
    public void ReturnsNullSeconds_WhenNoEndDate()
    {
        // Arrange
        var dto = BundlesControllerTestHarness.CreateDto(null);

        // Act
        var response = BundlesController.BundleResponse.FromDto(dto, DateTimeOffset.UtcNow);

        // Assert
        response.SecondsRemaining.ShouldBeNull();
    }
}