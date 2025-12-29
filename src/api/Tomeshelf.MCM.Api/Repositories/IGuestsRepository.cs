using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Domain.Entities.Mcm;
using Tomeshelf.Mcm.Api.Records;

namespace Tomeshelf.Mcm.Api.Repositories;

/// <summary>
///     Defines a contract for accessing and managing guest information associated with events.
/// </summary>
/// <remarks>
///     The repository provides asynchronous methods for retrieving event and guest data, as well as
///     persisting changes to the underlying data store. Implementations are expected to handle data retrieval, paging, and
///     persistence in a manner appropriate to the application's storage technology. All methods support cancellation via a
///     cancellation token.
/// </remarks>
public interface IGuestsRepository
{
    /// <summary>
    ///     Adds a new guest entity to the current context for insertion.
    /// </summary>
    /// <param name="guest">The guest entity to add. Cannot be null.</param>
    void AddGuest(GuestEntity guest);

    /// <summary>
    ///     Asynchronously retrieves the event with the specified identifier, including its associated guests.
    /// </summary>
    /// <param name="eventId">The unique identifier of the event to retrieve. Cannot be null or empty.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains the event entity with its guests, or
    ///     null if no event with the specified identifier exists.
    /// </returns>
    Task<EventEntity> GetEventWithGuestsAsync(string eventId, CancellationToken cancellationToken);

    /// <summary>
    ///     Asynchronously retrieves a paged list of guests for the specified event.
    /// </summary>
    /// <param name="eventId">The unique identifier of the event for which to retrieve guests. Cannot be null or empty.</param>
    /// <param name="page">The zero-based index of the page to retrieve. Must be greater than or equal to 0.</param>
    /// <param name="pageSize">The maximum number of guests to include in the page. Must be greater than 0.</param>
    /// <param name="includeDeleted">true to include guests that have been marked as deleted; otherwise, false.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a GuestSnapshot object with the guests
    ///     for the specified page. If no guests are found, the collection in the snapshot will be empty.
    /// </returns>
    Task<GuestSnapshot> GetPageAsync(string eventId, int page, int pageSize, bool includeDeleted, CancellationToken cancellationToken);

    /// <summary>
    ///     Asynchronously saves all changes made in this context to the underlying data store.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous save operation.</param>
    /// <returns>A task that represents the asynchronous save operation.</returns>
    Task SaveChangesAsync(CancellationToken cancellationToken);
}