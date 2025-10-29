using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Tomeshelf.Infrastructure.Fitness.Models;

public sealed class SleepResponse
{
    [JsonPropertyName("sleep")]
    public IReadOnlyList<SleepEntry> Entries { get; init; }

    public sealed class SleepEntry
    {
        [JsonPropertyName("dateOfSleep")]
        public string DateOfSleep { get; init; }

        [JsonPropertyName("startTime")]
        public string StartTime { get; init; }

        [JsonPropertyName("endTime")]
        public string EndTime { get; init; }

        [JsonPropertyName("duration")]
        public long? DurationMilliseconds { get; init; }

        [JsonPropertyName("minutesAsleep")]
        public int? MinutesAsleep { get; init; }

        [JsonPropertyName("minutesAwake")]
        public int? MinutesAwake { get; init; }

        [JsonPropertyName("minutesAfterWakeup")]
        public int? MinutesAfterWakeup { get; init; }

        [JsonPropertyName("minutesToFallAsleep")]
        public int? MinutesToFallAsleep { get; init; }

        [JsonPropertyName("efficiency")]
        public int? Efficiency { get; init; }

        [JsonPropertyName("levels")]
        public SleepLevels Levels { get; init; }
    }

    public sealed class SleepLevels
    {
        [JsonPropertyName("summary")]
        public SleepLevelSummary Summary { get; init; }

        [JsonPropertyName("data")]
        public IReadOnlyList<SleepLevelData> Data { get; init; }
    }

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

    public sealed class SleepLevelSummaryItem
    {
        [JsonPropertyName("count")]
        public int? Count { get; init; }

        [JsonPropertyName("minutes")]
        public int? Minutes { get; init; }

        [JsonPropertyName("thirtyDayAvgMinutes")]
        public int? ThirtyDayAverageMinutes { get; init; }
    }

    public sealed class SleepLevelData
    {
        [JsonPropertyName("dateTime")]
        public string DateTime { get; init; }

        [JsonPropertyName("level")]
        public string Level { get; init; }

        [JsonPropertyName("seconds")]
        public int? Seconds { get; init; }
    }
}