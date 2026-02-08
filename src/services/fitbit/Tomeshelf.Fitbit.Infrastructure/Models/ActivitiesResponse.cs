using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Tomeshelf.Fitbit.Infrastructure.Models;

public sealed class ActivitiesResponse
{
    [JsonPropertyName("summary")]
    public ActivitiesSummary Summary { get; init; }

    public sealed class ActivitiesSummary
    {
        [JsonPropertyName("steps")]
        public int? Steps { get; init; }

        [JsonPropertyName("floors")]
        public int? Floors { get; init; }

        [JsonPropertyName("caloriesOut")]
        public int? CaloriesOut { get; init; }

        [JsonPropertyName("distances")]
        public IReadOnlyList<ActivityDistance> Distances { get; init; }
    }

    public sealed class ActivityDistance
    {
        [JsonPropertyName("activity")]
        public string Activity { get; init; }

        [JsonPropertyName("distance")]
        public double? Distance { get; init; }
    }
}