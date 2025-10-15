#nullable enable
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Tomeshelf.Infrastructure.Fitness.Models;

public sealed class CaloriesInResponse
{
    [JsonPropertyName("foods-log-caloriesIn")]
    public IReadOnlyList<CaloriesEntry>? Entries { get; init; }

    public sealed class CaloriesEntry
    {
        [JsonPropertyName("dateTime")]
        public string? DateTime { get; init; }

        [JsonPropertyName("value")]
        public string? Value { get; init; }
    }
}