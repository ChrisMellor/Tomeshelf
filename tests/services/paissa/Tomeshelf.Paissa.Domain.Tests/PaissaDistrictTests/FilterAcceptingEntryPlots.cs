using FluentAssertions;
using Tomeshelf.Paissa.Domain.Entities;
using Tomeshelf.Paissa.Domain.ValueObjects;

namespace Tomeshelf.Paissa.Domain.Tests.PaissaDistrictTests;

public class FilterAcceptingEntryPlots
{
    [Fact]
    public void RespectsKnownSizeFlag()
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
        var requireKnown = district.FilterAcceptingEntryPlots(true);
        var allowUnknown = district.FilterAcceptingEntryPlots(false);

        // Assert
        requireKnown.Should()
                    .NotBeNull();
        requireKnown!.OpenPlots
                     .Should()
                     .HaveCount(1);
        requireKnown.OpenPlots[0]
                    .PlotNumber
                    .Should()
                    .Be(2);
        allowUnknown.Should()
                    .NotBeNull();
        allowUnknown!.OpenPlots
                     .Should()
                     .HaveCount(2);
    }

    [Fact]
    public void WhenNoneMatch_ReturnsNull()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var plots = new List<PaissaPlot> { PaissaPlot.Create(1, 1, HousingPlotSize.Small, 100, now, PurchaseSystem.Personal, 2, LotteryPhase.ResultsProcessing) };
        var district = PaissaDistrict.Create(1, "Mist", plots);

        // Act
        var result = district.FilterAcceptingEntryPlots(true);

        // Assert
        result.Should()
              .BeNull();
    }
}