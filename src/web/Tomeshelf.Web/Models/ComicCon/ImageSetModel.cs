using System.Text.Json.Serialization;

namespace Tomeshelf.Web.Models.ComicCon;

public sealed record ImageSetModel
{
    [JsonPropertyName("big")]
    public string Big { get; init; }

    [JsonPropertyName("med")]
    public string Med { get; init; }

    [JsonPropertyName("small")]
    public string Small { get; init; }

    [JsonPropertyName("thumb")]
    public string Thumb { get; init; }
}