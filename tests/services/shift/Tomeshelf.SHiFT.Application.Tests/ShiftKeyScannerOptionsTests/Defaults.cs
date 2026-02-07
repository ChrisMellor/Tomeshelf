using Tomeshelf.SHiFT.Application;

namespace Tomeshelf.SHiFT.Application.Tests.ShiftKeyScannerOptionsTests;

public class Defaults
{
    [Fact]
    public void Defaults_AreExpected()
    {
        // Arrange
        var options = new ShiftKeyScannerOptions();

        // Act
        var lookback = options.LookbackHours;
        var x = options.X;

        // Assert
        lookback.ShouldBe(24);
        x.Enabled.ShouldBeTrue();
        x.ApiBaseV2.ShouldBe("https://api.x.com/2/");
        x.OAuthTokenEndpoint.ShouldBe("https://api.x.com/oauth2/token");
        x.TokenCacheMinutes.ShouldBe(55);
        x.MaxPages.ShouldBe(4);
        x.MaxResultsPerPage.ShouldBe(100);
        x.ExcludeReplies.ShouldBeTrue();
        x.ExcludeRetweets.ShouldBeFalse();
    }
}
