using FluentAssertions;
using Tomeshelf.HumbleBundle.Domain.HumbleBundle;

namespace Tomeshelf.HumbleBundle.Domain.Tests.BundleTests;

public class GetRemainingTime
{
    [Fact]
    public void WhenEndsAtIsInTheFuture_ReturnsPositiveTimeSpan()
    {
        // Arrange
        var bundle = new Bundle { EndsAt = DateTimeOffset.UtcNow.AddHours(1) };

        // Act
        var remainingTime = bundle.GetRemainingTime();

        // Assert
        remainingTime.Should()
                     .HaveValue();
        remainingTime!.Value
                      .TotalHours
                      .Should()
                      .BeGreaterThan(0);
    }

    [Fact]
    public void WhenEndsAtIsInThePast_ReturnsNegativeTimeSpan()
    {
        // Arrange
        var bundle = new Bundle { EndsAt = DateTimeOffset.UtcNow.AddHours(-1) };

        // Act
        var remainingTime = bundle.GetRemainingTime();

        // Assert
        remainingTime.Should()
                     .HaveValue();
        remainingTime!.Value
                      .TotalHours
                      .Should()
                      .BeLessThan(0);
    }

    [Fact]
    public void WhenEndsAtIsNull_ReturnsNull()
    {
        // Arrange
        var bundle = new Bundle();

        // Act
        var remainingTime = bundle.GetRemainingTime();

        // Assert
        remainingTime.Should()
                     .NotHaveValue();
    }
}