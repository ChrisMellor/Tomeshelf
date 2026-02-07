using Shouldly;
using Tomeshelf.HumbleBundle.Api.Controllers;
using Tomeshelf.HumbleBundle.Api.Tests.TestUtilities;

namespace Tomeshelf.HumbleBundle.Api.Tests.Controllers.BundleResponseTests;

public class FromDto
{
    [Fact]
    public void ReturnsNullSeconds_WhenAlreadyExpired()
    {
        var now = DateTimeOffset.UtcNow;
        var dto = BundlesControllerTestHarness.CreateDto(now.AddMinutes(-5));

        var response = BundlesController.BundleResponse.FromDto(dto, now);

        response.SecondsRemaining.ShouldBeNull();
    }

    [Fact]
    public void ReturnsNullSeconds_WhenNoEndDate()
    {
        var dto = BundlesControllerTestHarness.CreateDto(null);

        var response = BundlesController.BundleResponse.FromDto(dto, DateTimeOffset.UtcNow);

        response.SecondsRemaining.ShouldBeNull();
    }
}