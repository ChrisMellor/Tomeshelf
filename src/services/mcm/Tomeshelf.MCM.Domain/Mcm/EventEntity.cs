using System;
using System.Collections.Generic;
using System.Linq;

namespace Tomeshelf.MCM.Domain.Mcm;

/// <summary>
///     Represents the configuration details for an event, including its unique identifier, name, and last update
///     timestamp.
/// </summary>
public sealed class EventEntity
{
    public EventEntity() : this(string.Empty, string.Empty)
    {
    }

    public EventEntity(string id, string name, ICollection<GuestEntity>? guests = null)
    {
        Id = id ?? string.Empty;
        Name = name ?? string.Empty;
        Guests = guests ?? new List<GuestEntity>();
    }

    /// <summary>
    ///     Gets or sets the unique identifier for the entity.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    ///     Gets or sets the name associated with the object.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     Gets or sets the date and time when the entity was last updated.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; set; }

    /// <summary>
    ///     Gets or sets the collection of guests associated with this entity.
    /// </summary>
    /// <remarks>
    ///     Modifying the collection directly affects the set of guests linked to the entity. The
    ///     collection must not be null.
    /// </remarks>
    public ICollection<GuestEntity> Guests { get; set; } = new List<GuestEntity>();

    /// <summary>
    ///     Calculates the number of unique guests associated with the event.
    /// </summary>
    /// <returns>The count of unique guests.</returns>
    public int GetUniqueGuestCount()
    {
        return Guests.Where(g => !g.IsDeleted)
                     .DistinctBy(g => (g.Information!.FirstName, g.Information.LastName))
                     .Count();
    }
}