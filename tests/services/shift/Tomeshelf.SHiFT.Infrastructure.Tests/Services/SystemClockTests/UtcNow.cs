using Shouldly;
using Tomeshelf.SHiFT.Infrastructure.Services;

namespace Tomeshelf.SHiFT.Infrastructure.Tests.Services.SystemClockTests;

public class UtcNow
{
    [Fact]
    public void ReturnsCurrentUtcTime()
    {
        var clock = new SystemClock();
        var before = DateTimeOffset.UtcNow;

        var now = clock.UtcNow;

        var after = DateTimeOffset.UtcNow;

        now.ShouldBeInRange(before, after);
    }
}