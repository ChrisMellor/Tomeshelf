using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Paissa.Application.Abstractions.Common;
using Tomeshelf.Paissa.Application.Abstractions.External;
using Tomeshelf.Paissa.Application.Features.Housing.Queries;
using Tomeshelf.Paissa.Domain.Entities;
using Tomeshelf.Paissa.Domain.ValueObjects;
using Xunit;

namespace Tomeshelf.Paissa.Application.Tests.Features.Housing.Queries;

public class GetAcceptingEntriesQueryHandlerTests
{
    private readonly Mock<IPaissaClient> _mockClient;
    private readonly Mock<IPaissaWorldSettings> _mockSettings;
    private readonly Mock<IClock> _mockClock;
    private readonly GetAcceptingEntriesQueryHandler _handler;

    public GetAcceptingEntriesQueryHandlerTests()
    {
        _mockClient = new Mock<IPaissaClient>();
        _mockSettings = new Mock<IPaissaWorldSettings>();
        _mockClock = new Mock<IClock>();
        _handler = new GetAcceptingEntriesQueryHandler(_mockClient.Object, _mockSettings.Object, _mockClock.Object);
    }

    [Fact]
    public async Task Handle_ValidQuery_ReturnsPaissaWorldSummary()
    {
        // Arrange
        var worldId = 1;
        var worldName = "Test World";
        var now = DateTimeOffset.UtcNow;

        _mockSettings.Setup(s => s.WorldId).Returns(worldId);
        _mockClock.Setup(c => c.UtcNow).Returns(now);

        var plots = new List<PaissaPlot>
        {
            PaissaPlot.Create(1, 1, HousingPlotSize.Small, 1000, now, PurchaseSystem.FreeCompany, 5, LotteryPhase.AcceptingEntries)
        };
        var districts = new List<PaissaDistrict>
        {
            PaissaDistrict.Create(1, "Test District", plots)
        };
        var world = PaissaWorld.Create(worldId, worldName, districts);

        _mockClient
            .Setup(c => c.GetWorldAsync(worldId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(world);

        var query = new GetAcceptingEntriesQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(worldId, result.WorldId);
        Assert.Equal(worldName, result.WorldName);
        Assert.Equal(now, result.RetrievedAtUtc);
        Assert.Single(result.Districts);
        _mockClient.Verify(c => c.GetWorldAsync(worldId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
