using System;
using Tomeshelf.HumbleBundle.Domain.HumbleBundle;
using Xunit;

namespace Tomeshelf.HumbleBundle.Domain.Tests;

public class BundleTests
{
    [Fact]
    public void GetRemainingTime_WhenEndsAtIsInTheFuture_ReturnsPositiveTimeSpan()
    {
        // Arrange
        var bundle = new Bundle
        {
            EndsAt = DateTimeOffset.UtcNow.AddHours(1)
        };

        // Act
        var remainingTime = bundle.GetRemainingTime();

        // Assert
        Assert.True(remainingTime.HasValue);
        Assert.True(remainingTime.Value.TotalHours > 0);
    }

    [Fact]
    public void GetRemainingTime_WhenEndsAtIsInThePast_ReturnsNegativeTimeSpan()
    {
        // Arrange
        var bundle = new Bundle
        {
            EndsAt = DateTimeOffset.UtcNow.AddHours(-1)
        };

        // Act
        var remainingTime = bundle.GetRemainingTime();

        // Assert
        Assert.True(remainingTime.HasValue);
        Assert.True(remainingTime.Value.TotalHours < 0);
    }

    [Fact]
    public void GetRemainingTime_WhenEndsAtIsNull_ReturnsNull()
    {
        // Arrange
        var bundle = new Bundle();

        // Act
        var remainingTime = bundle.GetRemainingTime();

        // Assert
        Assert.False(remainingTime.HasValue);
    }
}
