using Shouldly;
using Tomeshelf.Paissa.Domain.Entities;
using Tomeshelf.Paissa.Domain.ValueObjects;

namespace Tomeshelf.Paissa.Domain.Tests.PaissaDistrictTests;

public class FilterAcceptingEntryPlots
{
    [Fact]
    public void RespectsKnownSizeFlag()
    {
        var now = DateTimeOffset.UtcNow;
        var plots = new List<PaissaPlot>
        {
            PaissaPlot.Create(1, 1, HousingPlotSize.Unknown, 100, now, PurchaseSystem.Personal, 1, LotteryPhase.AcceptingEntries),
            PaissaPlot.Create(1, 2, HousingPlotSize.Small, 200, now, PurchaseSystem.Personal, 2, LotteryPhase.AcceptingEntries)
        };
        var district = PaissaDistrict.Create(1, "Mist", plots);

        var requireKnown = district.FilterAcceptingEntryPlots(true);
        var allowUnknown = district.FilterAcceptingEntryPlots(false);

        requireKnown.ShouldNotBeNull();
        requireKnown!.OpenPlots.ShouldHaveSingleItem();
        requireKnown.OpenPlots[0]
                    .PlotNumber
                    .ShouldBe(2);
        allowUnknown.ShouldNotBeNull();
        allowUnknown!.OpenPlots.Count.ShouldBe(2);
    }

    [Fact]
    public void WhenNoneMatch_ReturnsNull()
    {
        var now = DateTimeOffset.UtcNow;
        var plots = new List<PaissaPlot> { PaissaPlot.Create(1, 1, HousingPlotSize.Small, 100, now, PurchaseSystem.Personal, 2, LotteryPhase.ResultsProcessing) };
        var district = PaissaDistrict.Create(1, "Mist", plots);

        var result = district.FilterAcceptingEntryPlots(true);

        result.ShouldBeNull();
    }
}