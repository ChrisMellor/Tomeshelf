using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Tomeshelf.MCM.Application.Mcm;

public sealed record EventDto
{
    [JsonPropertyName("event_id")]
    public string? EventId { get; init; }

    [JsonPropertyName("event_name")]
    public string? EventName { get; init; }

    [JsonPropertyName("event_slug")]
    public string? EventSlug { get; init; }

    [JsonPropertyName("people")]
    public List<PersonDto> People { get; init; } = [];
}