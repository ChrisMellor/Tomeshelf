using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.MCM.Api.Records;

namespace Tomeshelf.MCM.Api.Repositories;

/// <summary>
///     Defines a contract for managing guest records and snapshots associated with events, including operations to
///     retrieve, update, and delete guest data asynchronously.
/// </summary>
/// <remarks>
///     Implementations of this interface are responsible for providing data access and persistence for
///     guest-related information in the context of events. All operations are asynchronous and support cancellation via a
///     CancellationToken. Methods typically operate on event-specific data, enabling efficient management of guest lists
///     and their changes over time.
/// </remarks>
public interface IGuestsRepository
{
    /// <summary>
    ///     Asynchronously deletes all records associated with the specified event identifier.
    /// </summary>
    /// <param name="eventId">The unique identifier of the event whose associated records are to be deleted.</param>
    /// <param name="cancellationToken">
    ///     A token to monitor for cancellation requests. The operation is canceled if the token is
    ///     triggered.
    /// </param>
    /// <returns>
    ///     A task that represents the asynchronous delete operation. The task result contains the number of records
    ///     deleted.
    /// </returns>
    Task<int> DeleteAllAsync(Guid eventId, CancellationToken cancellationToken);

    /// <summary>
    ///     Asynchronously retrieves a paged list of guests for the specified event.
    /// </summary>
    /// <param name="eventId">The unique identifier of the event for which to retrieve guest information.</param>
    /// <param name="page">The zero-based index of the page to retrieve. Must be greater than or equal to 0.</param>
    /// <param name="pageSize">The maximum number of guests to include in the page. Must be greater than 0.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a snapshot of guests for the
    ///     specified page. If no guests are found, the snapshot will contain an empty collection.
    /// </returns>
    Task<GuestSnapshot> GetPageAsync(Guid eventId, int page, int pageSize, CancellationToken cancellationToken);

    /// <summary>
    ///     Creates or updates a snapshot of guest records for the specified event asynchronously.
    /// </summary>
    /// <param name="eventId">The unique identifier of the event for which the guest snapshot is to be upserted.</param>
    /// <param name="guests">A read-only list of guest records to include in the snapshot. Cannot be null.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a SyncDelta object describing the
    ///     changes applied to the snapshot.
    /// </returns>
    Task<SyncDelta> UpsertSnapshotAsync(Guid eventId, IReadOnlyList<GuestRecord> guests, CancellationToken cancellationToken);
}