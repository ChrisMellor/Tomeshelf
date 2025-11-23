using System.Text.Json.Serialization;

namespace Tomeshelf.Infrastructure.Domains.Fitness.Models;

public sealed class WeightEntry
{
    [JsonPropertyName("date")]
    public string Date { get; init; }

    [JsonPropertyName("time")]
    public string Time { get; init; }

    [JsonPropertyName("weight")]
    public double Weight { get; init; }

    [JsonPropertyName("fat")]
    public double BodyFatPercentage { get; init; }

    [JsonPropertyName("leanMass")]
    public double LeanMassKg { get; init; }
}