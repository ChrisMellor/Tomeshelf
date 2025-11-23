using System.Text.Json.Serialization;

namespace Tomeshelf.Infrastructure.Domains.Fitness.Models;

public sealed class SleepLevelSummaryItem
{
    [JsonPropertyName("count")]
    public int Count { get; init; }

    [JsonPropertyName("minutes")]
    public int Minutes { get; init; }

    [JsonPropertyName("thirtyDayAvgMinutes")]
    public int ThirtyDayAverageMinutes { get; init; }
}