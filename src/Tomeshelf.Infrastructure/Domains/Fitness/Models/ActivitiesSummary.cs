using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Tomeshelf.Infrastructure.Domains.Fitness.Models;

public sealed class ActivitiesSummary
{
    [JsonPropertyName("steps")]
    public int Steps { get; init; }

    [JsonPropertyName("floors")]
    public int Floors { get; init; }

    [JsonPropertyName("caloriesOut")]
    public int CaloriesOut { get; init; }

    [JsonPropertyName("distances")]
    public IReadOnlyList<ActivityDistance> Distances { get; init; }
}