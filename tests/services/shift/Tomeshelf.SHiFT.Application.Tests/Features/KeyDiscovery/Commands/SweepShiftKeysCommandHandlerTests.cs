using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.SHiFT.Application.Abstractions.Common;
using Tomeshelf.SHiFT.Application.Abstractions.External;
using Tomeshelf.SHiFT.Application.Features.KeyDiscovery.Commands;
using Tomeshelf.SHiFT.Application.Features.KeyDiscovery.Models;
using Tomeshelf.SHiFT.Application.Features.Redemption.Models;
using Tomeshelf.SHiFT.Application.Features.Redemption.Redeem;
using Xunit;

namespace Tomeshelf.SHiFT.Application.Tests.Features.KeyDiscovery.Commands;

public class SweepShiftKeysCommandHandlerTests
{
    [Fact]
    public async Task Handle_GroupsCodesAndBuildsSummary()
    {
        // Arrange
        var clock = new Mock<IClock>();
        var gearbox = new Mock<IGearboxClient>();
        var sourceA = new Mock<IShiftKeySource>();
        var sourceB = new Mock<IShiftKeySource>();

        var now = DateTimeOffset.UtcNow;
        clock.Setup(c => c.UtcNow).Returns(now);

        sourceA.Setup(s => s.GetKeysAsync(It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(new List<ShiftKeyCandidate>
               {
                   new("abcde-fghij-klmno-pqrst-uvwxy", "Reddit", now)
               });

        sourceB.Setup(s => s.GetKeysAsync(It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(new List<ShiftKeyCandidate>
               {
                   new("ABCDE-FGHIJ-KLMNO-PQRST-UVWXY", "twitter", now),
                   new("11111-22222-33333-44444-55555", "Blog", now)
               });

        gearbox.Setup(g => g.RedeemCodeAsync("ABCDE-FGHIJ-KLMNO-PQRST-UVWXY", It.IsAny<CancellationToken>()))
               .ReturnsAsync(new List<RedeemResult>
               {
                   new(1, "a@example.com", "psn", true, null, null),
                   new(2, "b@example.com", "psn", false, RedeemErrorCode.RedemptionFailed, "used")
               });

        gearbox.Setup(g => g.RedeemCodeAsync("11111-22222-33333-44444-55555", It.IsAny<CancellationToken>()))
               .ReturnsAsync(new List<RedeemResult>
               {
                   new(3, "c@example.com", "steam", true, null, null)
               });

        var handler = new SweepShiftKeysCommandHandler(new[] { sourceA.Object, sourceB.Object }, gearbox.Object, clock.Object);

        // Act
        var result = await handler.Handle(new SweepShiftKeysCommand(TimeSpan.FromMinutes(5)), CancellationToken.None);

        // Assert
        Assert.Equal(now - TimeSpan.FromHours(1), result.SinceUtc);
        Assert.Equal(2, result.Items.Count);

        var first = result.Items.First(item => item.Code == "ABCDE-FGHIJ-KLMNO-PQRST-UVWXY");
        Assert.Equal(new[] { "Reddit", "twitter" }, first.Sources);
        Assert.Equal(2, first.Results.Count);

        Assert.Equal(2, result.Summary.TotalKeys);
        Assert.Equal(3, result.Summary.TotalRedemptionAttempts);
        Assert.Equal(2, result.Summary.TotalSucceeded);
        Assert.Equal(1, result.Summary.TotalFailed);
    }
}
