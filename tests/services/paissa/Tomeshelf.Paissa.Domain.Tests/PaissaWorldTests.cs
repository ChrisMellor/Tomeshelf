using System;
using System.Collections.Generic;
using Tomeshelf.Paissa.Domain.Entities;
using Tomeshelf.Paissa.Domain.ValueObjects;
using Xunit;

namespace Tomeshelf.Paissa.Domain.Tests;

public class PaissaWorldTests
{
    [Fact]
    public void FilterAcceptingEntryDistricts_ReturnsWorldWithFilteredDistricts()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var acceptingPlot = PaissaPlot.Create(1, 1, HousingPlotSize.Small, 100, now, PurchaseSystem.Personal, 1, LotteryPhase.AcceptingEntries);
        var closedPlot = PaissaPlot.Create(1, 2, HousingPlotSize.Small, 100, now, PurchaseSystem.Personal, 1, LotteryPhase.WinnersAnnounced);

        var acceptingDistrict = PaissaDistrict.Create(1, "Lavender", new List<PaissaPlot> { acceptingPlot });
        var closedDistrict = PaissaDistrict.Create(2, "Mist", new List<PaissaPlot> { closedPlot });

        var world = PaissaWorld.Create(10, "Alpha", new List<PaissaDistrict> { acceptingDistrict, closedDistrict });

        // Act
        var result = world.FilterAcceptingEntryDistricts(requireKnownSize: true);

        // Assert
        Assert.Equal(10, result.Id);
        Assert.Equal("Alpha", result.Name);
        Assert.Single(result.Districts);
        Assert.Equal("Lavender", result.Districts[0].Name);
    }
}
