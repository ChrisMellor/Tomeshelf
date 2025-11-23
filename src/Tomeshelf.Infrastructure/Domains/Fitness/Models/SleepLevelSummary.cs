using System.Text.Json.Serialization;

namespace Tomeshelf.Infrastructure.Domains.Fitness.Models;

public sealed class SleepLevelSummary
{
    [JsonPropertyName("deep")]
    public SleepLevelSummaryItem Deep { get; init; }

    [JsonPropertyName("light")]
    public SleepLevelSummaryItem Light { get; init; }

    [JsonPropertyName("rem")]
    public SleepLevelSummaryItem Rem { get; init; }

    [JsonPropertyName("wake")]
    public SleepLevelSummaryItem Wake { get; init; }
}