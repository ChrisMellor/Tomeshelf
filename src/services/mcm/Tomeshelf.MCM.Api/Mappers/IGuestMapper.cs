using Tomeshelf.Domain.Shared.Entities.Mcm;

namespace Tomeshelf.MCM.Api.Mappers;

/// <summary>
///     Defines methods for retrieving, cloning, and updating guest entities within an event management context.
/// </summary>
public interface IGuestMapper
{
    /// <summary>
    ///     Creates a new guest entity for the specified event by cloning the details from an existing guest entity.
    /// </summary>
    /// <param name="eventId">The unique identifier of the event for which the guest entity is to be created.</param>
    /// <param name="source">The guest entity whose details are to be cloned. Cannot be null.</param>
    /// <returns>
    ///     A new GuestEntity instance associated with the specified event, containing the cloned details from the source
    ///     entity.
    /// </returns>
    GuestEntity CloneForEvent(string eventId, GuestEntity source);

    /// <summary>
    ///     Retrieves the unique key associated with the specified guest.
    /// </summary>
    /// <param name="guest">The guest entity for which to obtain the unique key. Cannot be null.</param>
    /// <returns>A string containing the unique key for the specified guest.</returns>
    string GetGuestKey(GuestEntity guest);

    /// <summary>
    ///     Updates the properties of the specified guest entity with values from another guest entity.
    /// </summary>
    /// <param name="target">The guest entity to be updated. Cannot be null.</param>
    /// <param name="source">The guest entity containing the updated values. Cannot be null.</param>
    /// <returns>true if the update was successful; otherwise, false.</returns>
    bool UpdateGuest(GuestEntity target, GuestEntity source);
}