using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Tomeshelf.Paissa.Infrastructure.Services.External;

/// <summary>
///     Represents a district within the Paissa system, including its identifier, name, and information about available
///     plots.
/// </summary>
/// <remarks>
///     This record is intended for data transfer scenarios and is immutable. It encapsulates details about a
///     district, such as the number of open plots and the time of the oldest available plot. All properties are populated
///     from external data sources and are not intended to be modified after initialization.
/// </remarks>
public sealed record PaissaDistrictDto
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; } = null!;

    [JsonPropertyName("num_open_plots")]
    public int NumOpenPlots { get; init; }

    [JsonPropertyName("oldest_plot_time")]
    public double OldestPlotTime { get; init; }

    [JsonPropertyName("open_plots")]
    public IReadOnlyList<PaissaPlotDto> OpenPlots { get; init; } = Array.Empty<PaissaPlotDto>();
}