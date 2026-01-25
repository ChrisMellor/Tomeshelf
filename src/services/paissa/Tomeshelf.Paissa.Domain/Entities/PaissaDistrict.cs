using System;
using System.Collections.Generic;
using System.Linq;

namespace Tomeshelf.Paissa.Domain.Entities;

/// <summary>
///     Represents an immutable district within the Paissa area, identified by a unique ID and containing a collection of
///     open plots.
/// </summary>
/// <remarks>
///     Once created, the properties of this record cannot be changed. This type is intended to provide a
///     snapshot of a district's state at a specific point in time.
/// </remarks>
public sealed record PaissaDistrict
{
    public int Id { get; }
    public string Name { get; }
    public IReadOnlyList<PaissaPlot> OpenPlots { get; }

    private PaissaDistrict(int id, string name, IReadOnlyList<PaissaPlot> openPlots)
    {
        Id = id;
        Name = name;
        OpenPlots = openPlots;
    }

    public static PaissaDistrict Create(int id, string name, IReadOnlyList<PaissaPlot> openPlots)
    {
        if (id <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(id), "District id must be positive.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("District name is required.", nameof(name));
        }

        if (openPlots is null)
        {
            throw new ArgumentNullException(nameof(openPlots));
        }

        if (openPlots.Any(plot => plot is null))
        {
            throw new ArgumentException("Open plots cannot contain null entries.", nameof(openPlots));
        }

        var plots = openPlots.ToList();

        return new PaissaDistrict(id, name, plots);
    }

    public PaissaDistrict? FilterAcceptingEntryPlots(bool requireKnownSize)
    {
        var plots = OpenPlots
            .Where(plot => plot.IsAcceptingEntries && (!requireKnownSize || plot.HasKnownSize))
            .ToList();

        return plots.Count == 0
            ? null
            : Create(Id, Name, plots);
    }
}
