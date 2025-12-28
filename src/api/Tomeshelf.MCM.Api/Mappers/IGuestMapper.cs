using System;
using Tomeshelf.Domain.Entities.Mcm;

namespace Tomeshelf.Mcm.Api.Mappers;

/// <summary>
///     Defines methods for mapping, cloning, and updating guest entities within the context of events.
/// </summary>
/// <remarks>
///     Implementations of this interface provide functionality to retrieve guest display names, clone guest
///     data for different events, and update guest information. These operations are intended to facilitate consistent
///     handling of guest entities across event-related workflows.
/// </remarks>
public interface IGuestMapper
{
    /// <summary>
    ///     Returns the full name of the specified guest, combining the first and last names if available.
    /// </summary>
    /// <param name="guest">The guest entity from which to retrieve the full name. Cannot be null.</param>
    /// <returns>
    ///     A string containing the guest's full name, or an empty string if both first and last names are missing or
    ///     whitespace.
    /// </returns>
    string GetGuestKey(GuestEntity guest);

    /// <summary>
    ///     Creates a new GuestEntity instance for the specified event by cloning the provided source guest's information.
    /// </summary>
    /// <remarks>
    ///     The cloned guest will have its IsDeleted property set to false, and new identifiers will be
    ///     generated for any missing or empty IDs in the source. The method does not modify the source instance.
    /// </remarks>
    /// <param name="eventId">The unique identifier of the event to associate with the cloned guest.</param>
    /// <param name="source">The GuestEntity instance to clone. Must not be null.</param>
    /// <returns>
    ///     A new GuestEntity instance containing the cloned information from the source, associated with the specified
    ///     event.
    /// </returns>
    GuestEntity CloneForEvent(Guid eventId, GuestEntity source);

    /// <summary>
    ///     Updates the properties of the specified target guest entity with values from the source guest entity.
    /// </summary>
    /// <remarks>
    ///     If the target entity is marked as deleted, this method restores it before applying updates.
    ///     The method ensures that the target entity and its related information and social properties are initialized as
    ///     needed. Only string properties are copied from the source to the target. No changes are made if the source
    ///     entity's Information property is null.
    /// </remarks>
    /// <param name="target">The guest entity to update. This object will be modified with values from the source entity.</param>
    /// <param name="source">
    ///     The guest entity containing the updated values to copy to the target entity. Must have a non-null Information
    ///     property.
    /// </param>
    /// <returns>true if any properties of the target entity were changed; otherwise, false.</returns>
    bool UpdateGuest(GuestEntity target, GuestEntity source);
}