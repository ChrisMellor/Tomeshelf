using System;
using Tomeshelf.Paissa.Domain.Entities;
using Tomeshelf.Paissa.Domain.ValueObjects;
using Xunit;

namespace Tomeshelf.Paissa.Domain.Tests;

public class PaissaPlotTests
{
    [Fact]
    public void Create_ValidParameters_SetsPropertiesAndFlags()
    {
        // Arrange
        var updated = DateTimeOffset.UtcNow;

        // Act
        var plot = PaissaPlot.Create(2, 15, HousingPlotSize.Medium, 123456, updated, PurchaseSystem.FreeCompany | PurchaseSystem.Personal, 12, LotteryPhase.AcceptingEntries);

        // Assert
        Assert.Equal(2, plot.WardNumber);
        Assert.Equal(15, plot.PlotNumber);
        Assert.Equal(HousingPlotSize.Medium, plot.Size);
        Assert.Equal(123456, plot.Price);
        Assert.Equal(updated, plot.LastUpdatedUtc);
        Assert.True(plot.IsAcceptingEntries);
        Assert.True(plot.HasKnownSize);
        Assert.True(plot.Eligibility.AllowsPersonal);
        Assert.True(plot.Eligibility.AllowsFreeCompany);
    }

    [Theory]
    [InlineData(0, 1, "ward")]
    [InlineData(1, 0, "plot")]
    public void Create_InvalidWardOrPlot_Throws(int wardNumber, int plotNumber, string _)
    {
        // Arrange
        var updated = DateTimeOffset.UtcNow;

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            PaissaPlot.Create(wardNumber, plotNumber, HousingPlotSize.Small, 0, updated, PurchaseSystem.None, 0, LotteryPhase.Unknown));
    }

    [Fact]
    public void Create_NegativePrice_Throws()
    {
        // Arrange
        var updated = DateTimeOffset.UtcNow;

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            PaissaPlot.Create(1, 1, HousingPlotSize.Small, -1, updated, PurchaseSystem.None, 0, LotteryPhase.Unknown));
    }

    [Fact]
    public void Create_DefaultTimestamp_Throws()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            PaissaPlot.Create(1, 1, HousingPlotSize.Small, 0, default, PurchaseSystem.None, 0, LotteryPhase.Unknown));
    }

    [Fact]
    public void Create_NegativeLotteryEntries_Throws()
    {
        // Arrange
        var updated = DateTimeOffset.UtcNow;

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            PaissaPlot.Create(1, 1, HousingPlotSize.Small, 0, updated, PurchaseSystem.None, -1, LotteryPhase.Unknown));
    }
}
