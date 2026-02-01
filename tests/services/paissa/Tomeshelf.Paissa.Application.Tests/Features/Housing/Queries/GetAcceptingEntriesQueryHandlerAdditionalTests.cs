using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Paissa.Application.Abstractions.Common;
using Tomeshelf.Paissa.Application.Abstractions.External;
using Tomeshelf.Paissa.Application.Features.Housing.Queries;
using Tomeshelf.Paissa.Domain.Entities;
using Tomeshelf.Paissa.Domain.ValueObjects;
using Xunit;

namespace Tomeshelf.Paissa.Application.Tests.Features.Housing.Queries;

public class GetAcceptingEntriesQueryHandlerAdditionalTests
{
    [Fact]
    public async Task Handle_OrdersDistrictsAndGroupsBySize()
    {
        // Arrange
        var client = new Mock<IPaissaClient>();
        var settings = new Mock<IPaissaWorldSettings>();
        var clock = new Mock<IClock>();
        var handler = new GetAcceptingEntriesQueryHandler(client.Object, settings.Object, clock.Object);

        var now = DateTimeOffset.UtcNow;
        settings.Setup(s => s.WorldId).Returns(7);
        clock.Setup(c => c.UtcNow).Returns(now);

        var plotLarge = PaissaPlot.Create(2, 10, HousingPlotSize.Large, 100, now, PurchaseSystem.Personal, 1, LotteryPhase.AcceptingEntries);
        var plotSmall = PaissaPlot.Create(1, 2, HousingPlotSize.Small, 200, now, PurchaseSystem.Personal, 2, LotteryPhase.AcceptingEntries);
        var plotMedium = PaissaPlot.Create(1, 1, HousingPlotSize.Medium, 300, now, PurchaseSystem.FreeCompany, 3, LotteryPhase.AcceptingEntries);

        var districtBeta = PaissaDistrict.Create(2, "beta", new List<PaissaPlot> { plotLarge, plotSmall });
        var districtAlpha = PaissaDistrict.Create(1, "Alpha", new List<PaissaPlot> { plotMedium });
        var world = PaissaWorld.Create(7, "World-7", new List<PaissaDistrict> { districtBeta, districtAlpha });

        client.Setup(c => c.GetWorldAsync(7, It.IsAny<CancellationToken>()))
              .ReturnsAsync(world);

        // Act
        var result = await handler.Handle(new GetAcceptingEntriesQuery(), CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Districts.Count);
        Assert.Equal("Alpha", result.Districts[0].Name);
        Assert.Equal("beta", result.Districts[1].Name);

        var sizeGroups = result.Districts[0].SizeGroups;
        Assert.Equal(3, sizeGroups.Count);
        Assert.Equal("Large", sizeGroups[0].Size);
        Assert.Equal("Medium", sizeGroups[1].Size);
        Assert.Equal("Small", sizeGroups[2].Size);

        var mediumGroup = sizeGroups[1];
        Assert.Single(mediumGroup.Plots);
        Assert.Equal(1, mediumGroup.Plots[0].Ward);
        Assert.Equal(1, mediumGroup.Plots[0].Plot);

        var betaSmallPlots = result.Districts[1].SizeGroups.First(group => group.Size == "Small").Plots;
        Assert.Single(betaSmallPlots);
        Assert.Equal(1, betaSmallPlots[0].Ward);
        Assert.Equal(2, betaSmallPlots[0].Plot);
    }

    [Fact]
    public async Task Handle_FiltersUnknownSizes_WhenRequireKnownSize()
    {
        // Arrange
        var client = new Mock<IPaissaClient>();
        var settings = new Mock<IPaissaWorldSettings>();
        var clock = new Mock<IClock>();
        var handler = new GetAcceptingEntriesQueryHandler(client.Object, settings.Object, clock.Object);

        var now = DateTimeOffset.UtcNow;
        settings.Setup(s => s.WorldId).Returns(3);
        clock.Setup(c => c.UtcNow).Returns(now);

        var plotUnknown = PaissaPlot.Create(1, 1, HousingPlotSize.Unknown, 100, now, PurchaseSystem.Personal, 1, LotteryPhase.AcceptingEntries);
        var plotKnown = PaissaPlot.Create(1, 2, HousingPlotSize.Small, 200, now, PurchaseSystem.Personal, 1, LotteryPhase.AcceptingEntries);

        var districtUnknown = PaissaDistrict.Create(1, "Unknown", new List<PaissaPlot> { plotUnknown });
        var districtKnown = PaissaDistrict.Create(2, "Known", new List<PaissaPlot> { plotKnown });
        var world = PaissaWorld.Create(3, "World-3", new List<PaissaDistrict> { districtUnknown, districtKnown });

        client.Setup(c => c.GetWorldAsync(3, It.IsAny<CancellationToken>()))
              .ReturnsAsync(world);

        // Act
        var result = await handler.Handle(new GetAcceptingEntriesQuery(), CancellationToken.None);

        // Assert
        Assert.Single(result.Districts);
        Assert.Equal("Known", result.Districts[0].Name);
    }
}
