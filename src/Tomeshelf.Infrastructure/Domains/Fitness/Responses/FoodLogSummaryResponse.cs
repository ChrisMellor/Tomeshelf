using System.Text.Json.Serialization;
using Tomeshelf.Infrastructure.Domains.Fitness.Models;

namespace Tomeshelf.Infrastructure.Domains.Fitness.Responses;

public sealed class FoodLogSummaryResponse
{
    [JsonPropertyName("summary")]
    public FoodSummary Summary { get; init; }
}