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

        var after = DateTimeOffset.UtcNow;

        // Assert
        now.ShouldBeInRange(before, after);
    }
}
