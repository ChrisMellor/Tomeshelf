using System.Collections.Generic;

namespace Tomeshelf.Paissa.Application.Features.Housing.Dtos;

/// <summary>
///     Group of plots for a specific size category.
/// </summary>
/// <param name="Size">Display label for the size.</param>
/// <param name="SizeKey">Stable key for the size (small, medium, large).</param>
/// <param name="Plots">Plots within the size group.</param>
public sealed record PaissaSizeGroupSummaryDto(string Size, string SizeKey, IReadOnlyList<PaissaPlotSummaryDto> Plots);
