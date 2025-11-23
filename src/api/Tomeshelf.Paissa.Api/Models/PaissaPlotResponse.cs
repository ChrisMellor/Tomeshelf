using System;

namespace Tomeshelf.Paissa.Api.Models;

/// <summary>
///     Plot information exposed to the web front-end.
/// </summary>
public sealed record PaissaPlotResponse
{
    /// <summary>
    ///     Plot information exposed to the web front-end.
    /// </summary>
    /// <param name="ward">Ward number.</param>
    /// <param name="plot">Plot number.</param>
    /// <param name="price">Price in gil.</param>
    /// <param name="entries">Number of lottery entries.</param>
    /// <param name="lastUpdatedUtc">Last update timestamp from PaissaDB.</param>
    /// <param name="allowsPersonal">Indicates whether personal buyers may bid.</param>
    /// <param name="allowsFreeCompany">Indicates whether free companies may bid.</param>
    /// <param name="isEligibilityUnknown">True when buyer eligibility is unknown.</param>
    public PaissaPlotResponse(int ward, int plot, long price, int entries, DateTimeOffset lastUpdatedUtc, bool allowsPersonal, bool allowsFreeCompany, bool isEligibilityUnknown)
    {
        Ward = ward;
        Plot = plot;
        Price = price;
        Entries = entries;
        LastUpdatedUtc = lastUpdatedUtc;
        AllowsPersonal = allowsPersonal;
        AllowsFreeCompany = allowsFreeCompany;
        IsEligibilityUnknown = isEligibilityUnknown;
    }

    /// <summary>Ward number.</summary>
    public int Ward { get; init; }

    /// <summary>Plot number.</summary>
    public int Plot { get; init; }

    /// <summary>Price in gil.</summary>
    public long Price { get; init; }

    /// <summary>Number of lottery entries.</summary>
    public int Entries { get; init; }

    /// <summary>Last update timestamp from PaissaDB.</summary>
    public DateTimeOffset LastUpdatedUtc { get; init; }

    /// <summary>Indicates whether personal buyers may bid.</summary>
    public bool AllowsPersonal { get; init; }

    /// <summary>Indicates whether free companies may bid.</summary>
    public bool AllowsFreeCompany { get; init; }

    /// <summary>True when buyer eligibility is unknown.</summary>
    public bool IsEligibilityUnknown { get; init; }

    public void Deconstruct(out int ward, out int plot, out long price, out int entries, out DateTimeOffset lastUpdatedUtc, out bool allowsPersonal, out bool allowsFreeCompany,
            out bool isEligibilityUnknown)
    {
        ward = Ward;
        plot = Plot;
        price = Price;
        entries = Entries;
        lastUpdatedUtc = LastUpdatedUtc;
        allowsPersonal = AllowsPersonal;
        allowsFreeCompany = AllowsFreeCompany;
        isEligibilityUnknown = IsEligibilityUnknown;
    }
}