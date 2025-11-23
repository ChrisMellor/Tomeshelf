using System.Collections.Generic;
using System.Text.Json.Serialization;
using Tomeshelf.Infrastructure.Domains.Fitness.Models;

namespace Tomeshelf.Infrastructure.Domains.Fitness.Responses;

public sealed class WeightResponse
{
    [JsonPropertyName("weight")]
    public IReadOnlyList<WeightEntry> Entries { get; init; }
}