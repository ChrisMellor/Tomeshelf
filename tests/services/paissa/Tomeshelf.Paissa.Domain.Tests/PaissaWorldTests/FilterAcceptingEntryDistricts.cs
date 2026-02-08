using Shouldly;
using Tomeshelf.Paissa.Domain.Entities;
using Tomeshelf.Paissa.Domain.ValueObjects;

namespace Tomeshelf.Paissa.Domain.Tests.PaissaWorldTests;

public class FilterAcceptingEntryDistricts
{
    [Fact]
    public void ReturnsWorldWithFilteredDistricts()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var acceptingPlot = PaissaPlot.Create(1, 1, HousingPlotSize.Small, 100, now, PurchaseSystem.Personal, 1, LotteryPhase.AcceptingEntries);
        var closedPlot = PaissaPlot.Create(1, 2, HousingPlotSize.Small, 100, now, PurchaseSystem.Personal, 1, LotteryPhase.WinnersAnnounced);
        var acceptingDistrict = PaissaDistrict.Create(1, "Lavender", new List<PaissaPlot> { acceptingPlot });
        var closedDistrict = PaissaDistrict.Create(2, "Mist", new List<PaissaPlot> { closedPlot });
        var world = PaissaWorld.Create(10, "Alpha", new List<PaissaDistrict>
        {
            acceptingDistrict,
            closedDistrict
        });

        // Act
        var result = world.FilterAcceptingEntryDistricts(true);

        // Assert
        result.Id.ShouldBe(10);
        result.Name.ShouldBe("Alpha");
        result.Districts.ShouldHaveSingleItem();
        result.Districts[0]
              .Name
              .ShouldBe("Lavender");
    }
}