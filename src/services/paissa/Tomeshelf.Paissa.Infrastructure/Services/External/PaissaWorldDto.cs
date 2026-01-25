using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Tomeshelf.Paissa.Infrastructure.Services.External;

/// <summary>
///     Represents a data transfer object that contains summary information about a Paissa world, including its identifier,
///     name, districts, number of open plots, and the time of the oldest available plot.
/// </summary>
/// <remarks>
///     This record is intended for serialization and deserialization of Paissa world data, providing an
///     immutable structure for transferring world-related information between application layers or external systems. All
///     properties are initialized with default values to ensure safe deserialization, and the Name property is required
///     and
///     must not be null.
/// </remarks>
public sealed record PaissaWorldDto
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; } = null!;

    [JsonPropertyName("districts")]
    public IReadOnlyList<PaissaDistrictDto> Districts { get; init; } = Array.Empty<PaissaDistrictDto>();

    [JsonPropertyName("num_open_plots")]
    public int NumOpenPlots { get; init; }

    [JsonPropertyName("oldest_plot_time")]
    public double OldestPlotTime { get; init; }
}
