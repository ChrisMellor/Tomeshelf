using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.MCM.Api.Records;

namespace Tomeshelf.MCM.Api.Repositories;

/// <summary>
///     Provides in-memory storage and management of guest records for events, supporting asynchronous operations to
///     retrieve, update, and delete guest data by event identifier.
/// </summary>
/// <remarks>
///     This repository is intended for scenarios where thread-safe, in-memory management of guest lists is
///     sufficient, such as testing or lightweight applications. All operations are performed asynchronously and are safe
///     for concurrent access. The repository maintains guest records per event and supports paged retrieval, snapshot
///     upserts, and bulk deletion. Data is not persisted beyond the lifetime of the repository instance.
/// </remarks>
public class GuestsRepository : IGuestsRepository
{
    private readonly ConcurrentDictionary<Guid, List<GuestRecord>> _store = new ConcurrentDictionary<Guid, List<GuestRecord>>();

    /// <summary>
    ///     Asynchronously deletes all items associated with the specified event identifier.
    /// </summary>
    /// <param name="eventId">The unique identifier of the event whose items are to be deleted.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the delete operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains the number of items that were
    ///     deleted. Returns 0 if no items were found for the specified event identifier.
    /// </returns>
    public Task<int> DeleteAllAsync(Guid eventId, CancellationToken cancellationToken)
    {
        if (_store.TryRemove(eventId, out var existing))
        {
            return Task.FromResult(existing.Count);
        }

        return Task.FromResult(0);
    }

    /// <summary>
    ///     Retrieves a paged list of guest records for the specified event.
    /// </summary>
    /// <param name="eventId">The unique identifier of the event for which to retrieve guest records.</param>
    /// <param name="page">The one-based page number to retrieve. Must be greater than or equal to 1.</param>
    /// <param name="pageSize">The maximum number of guest records to include in the page. Must be greater than 0.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a snapshot of the total number of
    ///     guests and the guest records for the specified page. If the page is beyond the available data, the list of guest
    ///     records will be empty.
    /// </returns>
    public Task<GuestSnapshot> GetPageAsync(Guid eventId, int page, int pageSize, CancellationToken cancellationToken)
    {
        var list = _store.GetOrAdd(eventId, _ => []);

        var total = list.Count;
        var skip = (page - 1) * pageSize;

        IReadOnlyList<GuestRecord> items = skip >= total
                ? Array.Empty<GuestRecord>()
                : list.Skip(skip)
                      .Take(pageSize)
                      .ToList();

        return Task.FromResult(new GuestSnapshot(total, items));
    }

    /// <summary>
    ///     Creates or updates the guest snapshot for the specified event and returns a summary of changes applied.
    /// </summary>
    /// <remarks>
    ///     If a snapshot for the specified event does not exist, it is created. Existing guest records are
    ///     compared to the provided list to determine additions, updates, and removals. The operation is idempotent for
    ///     identical input.
    /// </remarks>
    /// <param name="eventId">The unique identifier of the event for which the guest snapshot is to be upserted.</param>
    /// <param name="guests">
    ///     A read-only list of guest records representing the desired state of the event's guest list. Each guest record
    ///     must have a unique key as determined by the implementation.
    /// </param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a SyncDelta object summarizing the
    ///     number of guests added, updated, removed, and the total count after the operation.
    /// </returns>
    public Task<SyncDelta> UpsertSnapshotAsync(Guid eventId, IReadOnlyList<GuestRecord> guests, CancellationToken cancellationToken)
    {
        var incomingByKey = guests.ToDictionary(KeyOf, g => g);

        var existing = _store.GetOrAdd(eventId, _ => []);
        var existingByKey = existing.ToDictionary(KeyOf, g => g);

        var added = 0;
        var updated = 0;
        var removed = 0;

        foreach (var (guestName, incomingGuestRecord) in incomingByKey)
        {
            if (!existingByKey.TryGetValue(guestName, out var existingGuestRecord))
            {
                added++;

                continue;
            }

            if (!Equals(existingGuestRecord, incomingGuestRecord))
            {
                updated++;
            }
        }

        foreach (var guestName in existingByKey.Keys)
        {
            if (!incomingByKey.ContainsKey(guestName))
            {
                removed++;
            }
        }

        _store[eventId] = guests.ToList();

        return Task.FromResult(new SyncDelta(added, updated, removed, _store[eventId].Count));
    }

    /// <summary>
    ///     Retrieves the unique key associated with the specified guest record.
    /// </summary>
    /// <param name="guestRecord">The guest record from which to obtain the key. Cannot be null.</param>
    /// <returns>A string representing the unique key for the guest record.</returns>
    private static string KeyOf(GuestRecord guestRecord)
    {
        return guestRecord.Name;
    }
}