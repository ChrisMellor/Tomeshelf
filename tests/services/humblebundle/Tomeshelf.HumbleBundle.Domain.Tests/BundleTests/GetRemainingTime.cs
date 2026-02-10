using Shouldly;
using Tomeshelf.HumbleBundle.Domain.HumbleBundle;

namespace Tomeshelf.HumbleBundle.Domain.Tests.BundleTests;

public class GetRemainingTime
{
    /// <summary>
    ///     Returns positive time span when the ends at is in the future.
    /// </summary>
    [Fact]
    public void WhenEndsAtIsInTheFuture_ReturnsPositiveTimeSpan()
    {
        // Arrange
        var bundle = new Bundle { EndsAt = DateTimeOffset.UtcNow.AddHours(1) };

        // Act
        var remainingTime = bundle.GetRemainingTime();

        // Assert
        remainingTime.HasValue.ShouldBeTrue();
        (remainingTime.Value.TotalHours > 0).ShouldBeTrue();
    }

    /// <summary>
    ///     Returns negative time span when the ends at is in the past.
    /// </summary>
    [Fact]
    public void WhenEndsAtIsInThePast_ReturnsNegativeTimeSpan()
    {
        // Arrange
        var bundle = new Bundle { EndsAt = DateTimeOffset.UtcNow.AddHours(-1) };

        // Act
        var remainingTime = bundle.GetRemainingTime();

        // Assert
        remainingTime.HasValue.ShouldBeTrue();
        (remainingTime.Value.TotalHours < 0).ShouldBeTrue();
    }

    /// <summary>
    ///     Returns null when the ends at is null.
    /// </summary>
    [Fact]
    public void WhenEndsAtIsNull_ReturnsNull()
    {
        // Arrange
        var bundle = new Bundle();

        // Act
        var remainingTime = bundle.GetRemainingTime();

        // Assert
        remainingTime.HasValue.ShouldBeFalse();
    }
}