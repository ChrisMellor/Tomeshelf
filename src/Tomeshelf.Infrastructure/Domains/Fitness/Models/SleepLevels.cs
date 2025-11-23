using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Tomeshelf.Infrastructure.Domains.Fitness.Models;

public sealed class SleepLevels
{
    [JsonPropertyName("summary")]
    public SleepLevelSummary Summary { get; init; }

    [JsonPropertyName("data")]
    public IReadOnlyList<SleepLevelData> Data { get; init; }
}