using System.Collections.Generic;

namespace Tomeshelf.Paissa.Api.Contracts;

/// <summary>
///     Group of plots for a specific size category.
/// </summary>
/// <param name="Size">Display label for the size.</param>
/// <param name="SizeKey">Stable key for the size (small, medium, large).</param>
/// <param name="Plots">Plots within the size group.</param>
public sealed record PaissaSizeGroupResponse(string Size, string SizeKey, IReadOnlyList<PaissaPlotResponse> Plots);