using System.Collections.Generic;
using System.Text.Json.Serialization;
using Tomeshelf.Infrastructure.Domains.Fitness.Models;

namespace Tomeshelf.Infrastructure.Domains.Fitness.Responses;

public sealed class SleepResponse
{
    [JsonPropertyName("sleep")]
    public IReadOnlyList<SleepEntry> Entries { get; init; }
}