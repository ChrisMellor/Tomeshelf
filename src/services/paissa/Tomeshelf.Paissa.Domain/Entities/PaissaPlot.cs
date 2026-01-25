using System;
using Tomeshelf.Paissa.Domain.ValueObjects;

namespace Tomeshelf.Paissa.Domain.Entities;

/// <summary>
///     Represents a plot of land within a specific ward, including its size, price, and associated lottery information.
/// </summary>
public sealed record PaissaPlot
{
    public int WardNumber { get; }
    public int PlotNumber { get; }
    public HousingPlotSize Size { get; }
    public long Price { get; }
    public DateTimeOffset LastUpdatedUtc { get; }
    public PurchaseSystem PurchaseSystem { get; }
    public int? LotteryEntries { get; }
    public LotteryPhase LotteryPhase { get; }

    private PaissaPlot(int wardNumber, int plotNumber, HousingPlotSize size, long price, DateTimeOffset lastUpdatedUtc, PurchaseSystem purchaseSystem, int? lotteryEntries, LotteryPhase lotteryPhase)
    {
        WardNumber = wardNumber;
        PlotNumber = plotNumber;
        Size = size;
        Price = price;
        LastUpdatedUtc = lastUpdatedUtc;
        PurchaseSystem = purchaseSystem;
        LotteryEntries = lotteryEntries;
        LotteryPhase = lotteryPhase;
    }

    public static PaissaPlot Create(int wardNumber, int plotNumber, HousingPlotSize size, long price, DateTimeOffset lastUpdatedUtc, PurchaseSystem purchaseSystem, int? lotteryEntries, LotteryPhase lotteryPhase)
    {
        if (wardNumber <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(wardNumber), "Ward number must be positive.");
        }

        if (plotNumber <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(plotNumber), "Plot number must be positive.");
        }

        if (price < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(price), "Price cannot be negative.");
        }

        if (lastUpdatedUtc == default)
        {
            throw new ArgumentException("Last updated time is required.", nameof(lastUpdatedUtc));
        }

        if (lotteryEntries is < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(lotteryEntries), "Lottery entries cannot be negative.");
        }

        return new PaissaPlot(wardNumber, plotNumber, size, price, lastUpdatedUtc, purchaseSystem, lotteryEntries, lotteryPhase);
    }

    public bool IsAcceptingEntries => LotteryPhase == LotteryPhase.AcceptingEntries;

    public bool HasKnownSize => Size != HousingPlotSize.Unknown;

    public PurchaseEligibility Eligibility => PurchaseSystem.ToEligibility();
}
