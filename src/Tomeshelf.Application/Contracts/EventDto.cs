using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Tomeshelf.Application.Contracts;

public sealed record EventDto
{
    [JsonPropertyName("event_id")]
    public string EventId { get; init; } = null!;

    [JsonPropertyName("event_name")]
    public string EventName { get; init; } = null!;

    [JsonPropertyName("event_slug")]
    public string EventSlug { get; init; } = null!;

    [JsonPropertyName("people")]
    public List<PersonDto> People { get; init; } = [];
}