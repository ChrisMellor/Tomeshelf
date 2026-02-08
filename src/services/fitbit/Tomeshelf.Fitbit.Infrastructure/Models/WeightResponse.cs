using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Tomeshelf.Fitbit.Infrastructure.Models;

public sealed class WeightResponse
{
    [JsonPropertyName("weight")]
    public IReadOnlyList<WeightEntry> Entries { get; init; }

    public sealed class WeightEntry
    {
        [JsonPropertyName("date")]
        public string Date { get; init; }

        [JsonPropertyName("time")]
        public string Time { get; init; }

        [JsonPropertyName("weight")]
        public double? Weight { get; init; }

        [JsonPropertyName("fat")]
        public double? BodyFatPercentage { get; init; }

        [JsonPropertyName("leanMass")]
        public double? LeanMassKg { get; init; }
    }
}