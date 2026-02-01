using FluentAssertions;
using Tomeshelf.Paissa.Domain.Entities;
using Tomeshelf.Paissa.Domain.ValueObjects;

namespace Tomeshelf.Paissa.Domain.Tests.PaissaPlotTests;

public class Create
{
    [Fact]
    public void ValidParameters_SetsPropertiesAndFlags()
    {
        // Arrange
        var updated = DateTimeOffset.UtcNow;

        // Act
        var plot = PaissaPlot.Create(2, 15, HousingPlotSize.Medium, 123456, updated, PurchaseSystem.FreeCompany | PurchaseSystem.Personal, 12, LotteryPhase.AcceptingEntries);

        // Assert
        plot.WardNumber.Should().Be(2);
        plot.PlotNumber.Should().Be(15);
        plot.Size.Should().Be(HousingPlotSize.Medium);
        plot.Price.Should().Be(123456);
        plot.LastUpdatedUtc.Should().Be(updated);
        plot.IsAcceptingEntries.Should().BeTrue();
        plot.HasKnownSize.Should().BeTrue();
        plot.Eligibility.AllowsPersonal.Should().BeTrue();
        plot.Eligibility.AllowsFreeCompany.Should().BeTrue();
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 0)]
    public void InvalidWardOrPlot_Throws(int wardNumber, int plotNumber)
    {
        // Arrange
        var updated = DateTimeOffset.UtcNow;

        // Act
        Action act = () => PaissaPlot.Create(wardNumber, plotNumber, HousingPlotSize.Small, 0, updated, PurchaseSystem.None, 0, LotteryPhase.Unknown);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void NegativePrice_Throws()
    {
        // Arrange
        var updated = DateTimeOffset.UtcNow;

        // Act
        Action act = () => PaissaPlot.Create(1, 1, HousingPlotSize.Small, -1, updated, PurchaseSystem.None, 0, LotteryPhase.Unknown);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void DefaultTimestamp_Throws()
    {
        // Arrange
        var updated = default(DateTimeOffset);

        // Act
        Action act = () => PaissaPlot.Create(1, 1, HousingPlotSize.Small, 0, updated, PurchaseSystem.None, 0, LotteryPhase.Unknown);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void NegativeLotteryEntries_Throws()
    {
        // Arrange
        var updated = DateTimeOffset.UtcNow;

        // Act
        Action act = () => PaissaPlot.Create(1, 1, HousingPlotSize.Small, 0, updated, PurchaseSystem.None, -1, LotteryPhase.Unknown);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
