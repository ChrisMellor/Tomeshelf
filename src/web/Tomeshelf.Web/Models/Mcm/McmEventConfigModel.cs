using System.Text.Json.Serialization;

namespace Tomeshelf.Web.Models.Mcm;

/// <summary>
///     Represents the configuration details for an MCM event, including its unique identifier and display name.
/// </summary>
public sealed record McmEventConfigModel
{
    /// <summary>
    ///     Gets the unique identifier for this instance.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    /// <summary>
    ///     Gets the name associated with this instance.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;
}