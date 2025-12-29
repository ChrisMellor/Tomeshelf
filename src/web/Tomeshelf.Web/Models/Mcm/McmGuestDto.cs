using System;
using System.Text.Json.Serialization;

namespace Tomeshelf.Web.Models.Mcm;

/// <summary>
///     Represents a data transfer object containing information about a guest in the MCM system.
/// </summary>
/// <remarks>
///     This record is typically used to transfer guest data between application layers or across service
///     boundaries. All properties are immutable and correspond to serialized JSON fields for interoperability.
/// </remarks>
public sealed record McmGuestDto
{
    /// <summary>
    ///     Gets the unique identifier for the entity.
    /// </summary>
    [JsonPropertyName("id")]
    public Guid Id { get; init; }

    /// <summary>
    ///     Gets the name associated with this instance.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    ///     Gets the descriptive text associated with the object.
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; init; } = string.Empty;

    /// <summary>
    ///     Gets the URL of the user's profile.
    /// </summary>
    [JsonPropertyName("profileUrl")]
    public string ProfileUrl { get; init; } = string.Empty;

    /// <summary>
    ///     Gets the URL of the associated image.
    /// </summary>
    [JsonPropertyName("imageUrl")]
    public string ImageUrl { get; init; } = string.Empty;

    /// <summary>
    ///     Gets the date and time when the item was added.
    /// </summary>
    [JsonPropertyName("addedAt")]
    public DateTimeOffset AddedAt { get; init; }

    /// <summary>
    ///     Gets the date and time when the item was removed, if applicable.
    /// </summary>
    [JsonPropertyName("removedAt")]
    public DateTimeOffset? RemovedAt { get; init; }

    /// <summary>
    ///     Gets a value indicating whether the entity has been marked as deleted.
    /// </summary>
    [JsonPropertyName("isDeleted")]
    public bool IsDeleted { get; init; }
}