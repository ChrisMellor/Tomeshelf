using System.Text.Json.Serialization;

namespace Tomeshelf.Application.Shared.Contracts.Mcm;

public sealed record VenueLocationDto
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = null!;

    [JsonPropertyName("name")]
    public string Name { get; init; } = null!;
}