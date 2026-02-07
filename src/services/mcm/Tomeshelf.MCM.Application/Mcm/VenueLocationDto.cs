using System.Text.Json.Serialization;

namespace Tomeshelf.MCM.Application.Mcm;

public sealed record VenueLocationDto
{
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }
}