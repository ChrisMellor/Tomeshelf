using System;
using Tomeshelf.Paissa.Domain.ValueObjects;

namespace Tomeshelf.Paissa.Domain.Entities;

/// <summary>
///     Represents a plot of land within a specific ward, including its size, price, and associated lottery information.
/// </summary>
public sealed record PaissaPlot
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="PaissaPlot" /> class.
    /// </summary>
    /// <param name="wardNumber">The ward number.</param>
    /// <param name="plotNumber">The plot number.</param>
    /// <param name="size">The size.</param>
    /// <param name="price">The price.</param>
    /// <param name="lastUpdatedUtc">The last updated utc.</param>
    /// <param name="purchaseSystem">The purchase system.</param>
    /// <param name="lotteryEntries">The lottery entries.</param>
    /// <param name="lotteryPhase">The lottery phase.</param>
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

    public int WardNumber { get; }

    public int PlotNumber { get; }

    public HousingPlotSize Size { get; }

    public long Price { get; }

    public DateTimeOffset LastUpdatedUtc { get; }

    public PurchaseSystem PurchaseSystem { get; }

    public int? LotteryEntries { get; }

    public LotteryPhase LotteryPhase { get; }

    public bool IsAcceptingEntries => LotteryPhase == LotteryPhase.AcceptingEntries;

    public bool HasKnownSize => Size != HousingPlotSize.Unknown;

    public PurchaseEligibility Eligibility => PurchaseSystem.ToEligibility();

    /// <summary>
    ///     Creates.
    /// </summary>
    /// <param name="wardNumber">The ward number.</param>
    /// <param name="plotNumber">The plot number.</param>
    /// <param name="size">The size.</param>
    /// <param name="price">The price.</param>
    /// <param name="lastUpdatedUtc">The last updated utc.</param>
    /// <param name="purchaseSystem">The purchase system.</param>
    /// <param name="lotteryEntries">The lottery entries.</param>
    /// <param name="lotteryPhase">The lottery phase.</param>
    /// <returns>The result of the operation.</returns>
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
}