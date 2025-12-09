using System;

namespace Tomeshelf.MCM.Api.Models;

/// <summary>
///     Represents a configuration entity with a unique identifier and a name.
/// </summary>
public class EventConfigModel
{
    /// <summary>
    ///     Gets or sets the unique identifier for the entity.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    ///     Gets or sets the name associated with the object.
    /// </summary>
    public string Name { get; init; }
}