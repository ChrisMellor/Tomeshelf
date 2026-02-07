using Shouldly;
using Tomeshelf.HumbleBundle.Domain.HumbleBundle;

namespace Tomeshelf.HumbleBundle.Domain.Tests.BundleTests;

public class GetRemainingTime
{
    [Fact]
    public void WhenEndsAtIsInTheFuture_ReturnsPositiveTimeSpan()
    {
        var bundle = new Bundle { EndsAt = DateTimeOffset.UtcNow.AddHours(1) };

        var remainingTime = bundle.GetRemainingTime();

        remainingTime.HasValue.ShouldBeTrue();
        (remainingTime.Value.TotalHours > 0).ShouldBeTrue();
    }

    [Fact]
    public void WhenEndsAtIsInThePast_ReturnsNegativeTimeSpan()
    {
        var bundle = new Bundle { EndsAt = DateTimeOffset.UtcNow.AddHours(-1) };

        var remainingTime = bundle.GetRemainingTime();

        remainingTime.HasValue.ShouldBeTrue();
        (remainingTime.Value.TotalHours < 0).ShouldBeTrue();
    }

    [Fact]
    public void WhenEndsAtIsNull_ReturnsNull()
    {
        var bundle = new Bundle();

        var remainingTime = bundle.GetRemainingTime();

        remainingTime.HasValue.ShouldBeFalse();
    }
}