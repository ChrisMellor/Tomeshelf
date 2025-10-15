#nullable enable
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Tomeshelf.Infrastructure.Fitness.Models;

public sealed class SleepResponse
{
    [JsonPropertyName("sleep")]
    public IReadOnlyList<SleepEntry>? Entries { get; init; }

    public sealed class SleepEntry
    {
        [JsonPropertyName("dateOfSleep")]
        public string? DateOfSleep { get; init; }

        [JsonPropertyName("startTime")]
        public string? StartTime { get; init; }

        [JsonPropertyName("endTime")]
        public string? EndTime { get; init; }

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
    }
}