using System.Text.Json.Serialization;

namespace Tomeshelf.Infrastructure.Domains.Fitness.Models;

public sealed class ActivityDistance
{
    [JsonPropertyName("activity")]
    public string Activity { get; init; }

    [JsonPropertyName("distance")]
    public double Distance { get; init; }
}