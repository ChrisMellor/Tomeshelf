using System.Text.Json.Serialization;

namespace Tomeshelf.Infrastructure.Fitness.Models;

public sealed class FoodLogSummaryResponse
{
    [JsonPropertyName("summary")]
    public FoodSummary Summary { get; init; }

    public sealed class FoodSummary
    {
        [JsonPropertyName("calories")]
        public int? Calories { get; init; }

        [JsonPropertyName("carbs")]
        public double? Carbs { get; init; }

        [JsonPropertyName("fat")]
        public double? Fat { get; init; }

        [JsonPropertyName("fiber")]
        public double? Fiber { get; init; }

        [JsonPropertyName("protein")]
        public double? Protein { get; init; }

        [JsonPropertyName("sodium")]
        public double? Sodium { get; init; }
    }
}