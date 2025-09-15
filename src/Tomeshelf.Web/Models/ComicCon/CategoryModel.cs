using System.Text.Json.Serialization;

namespace Tomeshelf.Web.Models.ComicCon;

public sealed record CategoryModel
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = null!;

    [JsonPropertyName("name")]
    public string Name { get; init; } = null!;

    [JsonPropertyName("color")]
    public string Color { get; init; }

}

