using System;
using System.Collections.Generic;
using Tomeshelf.Paissa.Domain.Entities;
using Tomeshelf.Paissa.Domain.ValueObjects;
using Xunit;

namespace Tomeshelf.Paissa.Domain.Tests;

public class PaissaDistrictFilterTests
{
    [Fact]
    public void FilterAcceptingEntryPlots_WhenNoneMatch_ReturnsNull()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var plots = new List<PaissaPlot>
        {
            PaissaPlot.Create(1, 1, HousingPlotSize.Small, 100, now, PurchaseSystem.Personal, 2, LotteryPhase.ResultsProcessing)
        };
        var district = PaissaDistrict.Create(1, "Mist", plots);

        // Act
        var result = district.FilterAcceptingEntryPlots(requireKnownSize: true);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void FilterAcceptingEntryPlots_RespectsKnownSizeFlag()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var plots = new List<PaissaPlot>
        {
            PaissaPlot.Create(1, 1, HousingPlotSize.Unknown, 100, now, PurchaseSystem.Personal, 1, LotteryPhase.AcceptingEntries),
            PaissaPlot.Create(1, 2, HousingPlotSize.Small, 200, now, PurchaseSystem.Personal, 2, LotteryPhase.AcceptingEntries)
        };
        var district = PaissaDistrict.Create(1, "Mist", plots);

        // Act
        var requireKnown = district.FilterAcceptingEntryPlots(requireKnownSize: true);
        var allowUnknown = district.FilterAcceptingEntryPlots(requireKnownSize: false);

        // Assert
        Assert.NotNull(requireKnown);
        Assert.Single(requireKnown!.OpenPlots);
        Assert.Equal(2, requireKnown.OpenPlots[0].PlotNumber);

        Assert.NotNull(allowUnknown);
        Assert.Equal(2, allowUnknown!.OpenPlots.Count);
    }
}
