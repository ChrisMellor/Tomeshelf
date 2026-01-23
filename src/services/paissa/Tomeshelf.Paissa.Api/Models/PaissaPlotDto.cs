using System.Text.Json.Serialization;

namespace Tomeshelf.Paissa.Api.Models;

public sealed record PaissaPlotDto
{
    [JsonPropertyName("world_id")]
    public int WorldId { get; init; }

    [JsonPropertyName("district_id")]
    public int DistrictId { get; init; }

    [JsonPropertyName("ward_number")]
    public int WardNumber { get; init; }

    [JsonPropertyName("plot_number")]
    public int PlotNumber { get; init; }

    [JsonPropertyName("size")]
    public int Size { get; init; }

    [JsonPropertyName("price")]
    public long Price { get; init; }

    [JsonPropertyName("last_updated_time")]
    public double LastUpdatedTime { get; init; }

    [JsonPropertyName("first_seen_time")]
    public double FirstSeenTime { get; init; }

    [JsonPropertyName("est_time_open_min")]
    public double EstTimeOpenMin { get; init; }

    [JsonPropertyName("est_time_open_max")]
    public double EstTimeOpenMax { get; init; }

    [JsonPropertyName("purchase_system")]
    public int PurchaseSystem { get; init; }

    [JsonPropertyName("lotto_entries")]
    public int? LotteryEntries { get; init; }

    [JsonPropertyName("lotto_phase")]
    public int? LotteryPhase { get; init; }

    [JsonPropertyName("lotto_phase_until")]
    public long? LotteryPhaseUntil { get; init; }
}