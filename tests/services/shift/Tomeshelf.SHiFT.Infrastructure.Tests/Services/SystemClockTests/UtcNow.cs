using Shouldly;
using Tomeshelf.SHiFT.Infrastructure.Services;

namespace Tomeshelf.SHiFT.Infrastructure.Tests.Services.SystemClockTests;

public class UtcNow
{
    [Fact]
    public void ReturnsCurrentUtcTime()
    {
        // Arrange
        var clock = new SystemClock();
        var before = DateTimeOffset.UtcNow;

        // Act
        var now = clock.UtcNow;

        // Assert
        var after = DateTimeOffset.UtcNow;

        now.ShouldBeInRange(before, after);
    }
}