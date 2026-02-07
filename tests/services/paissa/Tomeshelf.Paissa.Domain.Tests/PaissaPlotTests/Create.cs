using Tomeshelf.Paissa.Domain.Entities;
using Tomeshelf.Paissa.Domain.ValueObjects;

namespace Tomeshelf.Paissa.Domain.Tests.PaissaPlotTests;

public class Create
{
    [Fact]
    public void DefaultTimestamp_Throws()
    {
        // Arrange
        var updated = default(DateTimeOffset);

        // Act
        // Act
        var exception = Should.Throw<ArgumentException>(() => PaissaPlot.Create(1, 1, HousingPlotSize.Small, 0, updated, PurchaseSystem.None, 0, LotteryPhase.Unknown));

        // Assert
        exception.ShouldNotBeNull();
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 0)]
    public void InvalidWardOrPlot_Throws(int wardNumber, int plotNumber)
    {
        // Arrange
        var updated = DateTimeOffset.UtcNow;

        // Act
        // Act
        var exception = Should.Throw<ArgumentOutOfRangeException>(() => PaissaPlot.Create(wardNumber, plotNumber, HousingPlotSize.Small, 0, updated, PurchaseSystem.None, 0, LotteryPhase.Unknown));

        // Assert
        exception.ShouldNotBeNull();
    }

    [Fact]
    public void NegativeLotteryEntries_Throws()
    {
        // Arrange
        var updated = DateTimeOffset.UtcNow;

        // Act
        // Act
        var exception = Should.Throw<ArgumentOutOfRangeException>(() => PaissaPlot.Create(1, 1, HousingPlotSize.Small, 0, updated, PurchaseSystem.None, -1, LotteryPhase.Unknown));

        // Assert
        exception.ShouldNotBeNull();
    }

    [Fact]
    public void NegativePrice_Throws()
    {
        // Arrange
        var updated = DateTimeOffset.UtcNow;

        // Act
        // Act
        var exception = Should.Throw<ArgumentOutOfRangeException>(() => PaissaPlot.Create(1, 1, HousingPlotSize.Small, -1, updated, PurchaseSystem.None, 0, LotteryPhase.Unknown));

        // Assert
        exception.ShouldNotBeNull();
    }

    [Fact]
    public void ValidParameters_SetsPropertiesAndFlags()
    {
        // Arrange
        var updated = DateTimeOffset.UtcNow;

        // Act
        var plot = PaissaPlot.Create(2, 15, HousingPlotSize.Medium, 123456, updated, PurchaseSystem.FreeCompany | PurchaseSystem.Personal, 12, LotteryPhase.AcceptingEntries);

        // Assert
        plot.WardNumber.ShouldBe(2);
        plot.PlotNumber.ShouldBe(15);
        plot.Size.ShouldBe(HousingPlotSize.Medium);
        plot.Price.ShouldBe(123456);
        plot.LastUpdatedUtc.ShouldBe(updated);
        plot.IsAcceptingEntries.ShouldBeTrue();
        plot.HasKnownSize.ShouldBeTrue();
        plot.Eligibility.AllowsPersonal.ShouldBeTrue();
        plot.Eligibility.AllowsFreeCompany.ShouldBeTrue();
    }
}
