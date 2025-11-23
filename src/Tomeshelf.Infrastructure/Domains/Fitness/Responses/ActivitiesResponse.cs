using System.Text.Json.Serialization;
using Tomeshelf.Infrastructure.Domains.Fitness.Models;

namespace Tomeshelf.Infrastructure.Domains.Fitness.Responses;

public sealed class ActivitiesResponse
{
    [JsonPropertyName("summary")]
    public ActivitiesSummary Summary { get; init; }
}