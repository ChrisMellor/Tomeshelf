using System.Text.Json.Serialization;

namespace Tomeshelf.Infrastructure.Domains.Fitness.Models;

public sealed class SleepLevelData
{
    [JsonPropertyName("dateTime")]
    public string DateTime { get; init; }

    [JsonPropertyName("level")]
    public string Level { get; init; }

    [JsonPropertyName("seconds")]
    public int Seconds { get; init; }
}