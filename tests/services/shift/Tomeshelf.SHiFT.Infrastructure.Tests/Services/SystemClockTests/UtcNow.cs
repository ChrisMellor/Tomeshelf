using Shouldly;
using Tomeshelf.SHiFT.Infrastructure.Services;

namespace Tomeshelf.SHiFT.Infrastructure.Tests.Services.SystemClockTests;

public class UtcNow
{
    /// <summary>
    ///     Returns the current utc time.
    /// </summary>
    [Fact]
    public void ReturnsCurrentUtcTime()
    {
        // Arrange
        var clock = new SystemClock();
        var before = DateTimeOffset.UtcNow;

        var now = clock.UtcNow;

        // Act
        var after = DateTimeOffset.UtcNow;

        // Assert
        now.ShouldBeInRange(before, after);
    }
}