using System.Text.Json.Serialization;

namespace Tomeshelf.MCM.Application.Mcm;

public sealed record CategoryDto
{
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("color")]
    public string? Color { get; init; }
}