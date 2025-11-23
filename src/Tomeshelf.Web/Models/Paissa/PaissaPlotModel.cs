using System;

namespace Tomeshelf.Web.Models.Paissa;

public sealed record PaissaPlotModel
{
    public PaissaPlotModel(int ward, int plot, long price, int entries, DateTimeOffset lastUpdatedUtc, bool allowsPersonal, bool allowsFreeCompany, bool isEligibilityUnknown)
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

    public int Ward { get; init; }

    public int Plot { get; init; }

    public long Price { get; init; }

    public int Entries { get; init; }

    public DateTimeOffset LastUpdatedUtc { get; init; }

    public bool AllowsPersonal { get; init; }

    public bool AllowsFreeCompany { get; init; }

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