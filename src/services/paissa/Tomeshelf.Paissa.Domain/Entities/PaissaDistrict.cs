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
    /// <summary>
    ///     Initializes a new instance of the <see cref="PaissaDistrict" /> class.
    /// </summary>
    /// <param name="id">The id.</param>
    /// <param name="name">The name.</param>
    /// <param name="openPlots">The open plots.</param>
    private PaissaDistrict(int id, string name, IReadOnlyList<PaissaPlot> openPlots)
    {
        Id = id;
        Name = name;
        OpenPlots = openPlots;
    }

    public int Id { get; }

    public string Name { get; }

    public IReadOnlyList<PaissaPlot> OpenPlots { get; }

    /// <summary>
    ///     Creates.
    /// </summary>
    /// <param name="id">The id.</param>
    /// <param name="name">The name.</param>
    /// <param name="openPlots">The open plots.</param>
    /// <returns>The result of the operation.</returns>
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

    /// <summary>
    ///     Filters the accepting entry plots.
    /// </summary>
    /// <param name="requireKnownSize">The require known size.</param>
    /// <returns>The result of the operation.</returns>
    public PaissaDistrict? FilterAcceptingEntryPlots(bool requireKnownSize)
    {
        var plots = OpenPlots.Where(plot => plot.IsAcceptingEntries && (!requireKnownSize || plot.HasKnownSize))
                             .ToList();

        return plots.Count == 0
            ? null
            : Create(Id, Name, plots);
    }
}