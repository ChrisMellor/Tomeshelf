using Moq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.SHiFT.Application.Abstractions.External;
using Tomeshelf.SHiFT.Application.Features.Redemption.Commands;
using Tomeshelf.SHiFT.Application.Features.Redemption.Redeem;
using Xunit;

namespace Tomeshelf.SHiFT.Application.Tests.Features.Redemption.Commands;

public class RedeemShiftCodeCommandHandlerTests
{
    [Fact]
    public async Task Handle_CallsGearboxClientAndReturnsResults()
    {
        // Arrange
        var gearbox = new Mock<IGearboxClient>();
        var handler = new RedeemShiftCodeCommandHandler(gearbox.Object);
        var expected = new List<RedeemResult> { new(1, "a@example.com", "psn", true, null, null) };

        gearbox.Setup(g => g.RedeemCodeAsync("CODE-12345-ABCDE-67890-FGHIJ", It.IsAny<CancellationToken>()))
               .ReturnsAsync(expected);

        // Act
        var result = await handler.Handle(new RedeemShiftCodeCommand("CODE-12345-ABCDE-67890-FGHIJ"), CancellationToken.None);

        // Assert
        Assert.Same(expected, result);
        gearbox.Verify(g => g.RedeemCodeAsync("CODE-12345-ABCDE-67890-FGHIJ", It.IsAny<CancellationToken>()), Times.Once);
    }
}
