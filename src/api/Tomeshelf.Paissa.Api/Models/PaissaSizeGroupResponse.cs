using System.Collections.Generic;

namespace Tomeshelf.Paissa.Api.Models;

/// <summary>
///     Group of plots for a specific size category.
/// </summary>
public sealed record PaissaSizeGroupResponse
{
    /// <summary>
    ///     Group of plots for a specific size category.
    /// </summary>
    /// <param name="size">Display label for the size.</param>
    /// <param name="sizeKey">Stable key for the size (small, medium, large).</param>
    /// <param name="plots">Plots within the size group.</param>
    public PaissaSizeGroupResponse(string size, string sizeKey, IReadOnlyList<PaissaPlotResponse> plots)
    {
        Size = size;
        SizeKey = sizeKey;
        Plots = plots;
    }

    /// <summary>Display label for the size.</summary>
    public string Size { get; init; }

    /// <summary>Stable key for the size (small, medium, large).</summary>
    public string SizeKey { get; init; }

    /// <summary>Plots within the size group.</summary>
    public IReadOnlyList<PaissaPlotResponse> Plots { get; init; }

    public void Deconstruct(out string size, out string sizeKey, out IReadOnlyList<PaissaPlotResponse> plots)
    {
        size = Size;
        sizeKey = SizeKey;
        plots = Plots;
    }
}