using System.Text.Json.Serialization;

namespace Tomeshelf.Application.Contracts;

public sealed record ImageSetDto
{
    [JsonPropertyName("big")] public string Big { get; init; }

    [JsonPropertyName("med")] public string Med { get; init; }

    [JsonPropertyName("small")] public string Small { get; init; }

    [JsonPropertyName("thumb")] public string Thumb { get; init; }
}