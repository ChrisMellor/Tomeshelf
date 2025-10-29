using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Tomeshelf.Paissa.Api.Models;

public sealed record PaissaWorldDto(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("districts")] IReadOnlyList<PaissaDistrictDto> Districts,
    [property: JsonPropertyName("num_open_plots")] int NumOpenPlots,
    [property: JsonPropertyName("oldest_plot_time")] double OldestPlotTime);

public sealed record PaissaDistrictDto(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("num_open_plots")] int NumOpenPlots,
    [property: JsonPropertyName("oldest_plot_time")] double OldestPlotTime,
    [property: JsonPropertyName("open_plots")] IReadOnlyList<PaissaPlotDto> OpenPlots);

public sealed record PaissaPlotDto(
    [property: JsonPropertyName("world_id")] int WorldId,
    [property: JsonPropertyName("district_id")] int DistrictId,
    [property: JsonPropertyName("ward_number")] int WardNumber,
    [property: JsonPropertyName("plot_number")] int PlotNumber,
    [property: JsonPropertyName("size")] int Size,
    [property: JsonPropertyName("price")] long Price,
    [property: JsonPropertyName("last_updated_time")] double LastUpdatedTime,
    [property: JsonPropertyName("first_seen_time")] double FirstSeenTime,
    [property: JsonPropertyName("est_time_open_min")] double EstTimeOpenMin,
    [property: JsonPropertyName("est_time_open_max")] double EstTimeOpenMax,
    [property: JsonPropertyName("purchase_system")] int PurchaseSystem,
    [property: JsonPropertyName("lotto_entries")] int? LotteryEntries,
    [property: JsonPropertyName("lotto_phase")] int? LotteryPhase,
    [property: JsonPropertyName("lotto_phase_until")] long? LotteryPhaseUntil);
