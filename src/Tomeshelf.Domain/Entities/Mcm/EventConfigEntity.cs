using System;

namespace Tomeshelf.Domain.Entities.Mcm;

/// <summary>
///     Represents the configuration details for an event, including its unique identifier, name, and last update
///     timestamp.
/// </summary>
public sealed class EventConfigEntity
{
    /// <summary>
    ///     Gets or sets the unique identifier for the entity.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///     Gets or sets the name associated with the object.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     Gets or sets the date and time when the entity was last updated.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; set; }
}