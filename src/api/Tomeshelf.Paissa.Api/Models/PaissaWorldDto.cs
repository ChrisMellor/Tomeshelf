using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Tomeshelf.Paissa.Api.Models;

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