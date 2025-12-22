using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Domain.Entities.Mcm;
using Tomeshelf.Infrastructure.Persistence;
using Tomeshelf.Mcm.Api.Records;

namespace Tomeshelf.Mcm.Api.Repositories;

/// <summary>
///     Provides methods for managing guest records associated with events, including retrieval, synchronization, and
///     deletion operations.
/// </summary>
/// <remarks>
///     This repository encapsulates data access logic for guest-related operations within the event context.
///     All methods are asynchronous and require a valid database context. Thread safety is determined by the underlying
///     database context implementation.
/// </remarks>
public class GuestsRepository : IGuestsRepository
{
    private readonly TomeshelfMcmDbContext _dbContext;

    /// <summary>
    ///     Initializes a new instance of the GuestsRepository class using the specified database context.
    /// </summary>
    /// <param name="dbContext">The database context to be used for data access operations. Cannot be null.</param>
    public GuestsRepository(TomeshelfMcmDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    ///     Asynchronously deletes all event records with the specified event identifier.
    /// </summary>
    /// <param name="eventId">
    ///     The unique identifier of the event to delete. Only records matching this identifier will be
    ///     removed.
    /// </param>
    /// <param name="cancellationToken">A token that can be used to cancel the delete operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the number of records deleted.</returns>
    public async Task<int> DeleteAllAsync(Guid eventId, CancellationToken cancellationToken)
    {
        return await _dbContext.Events.Where(x => x.Id == eventId)
                               .ExecuteDeleteAsync(cancellationToken);
    }

    /// <summary>
    ///     Asynchronously retrieves a paged list of guest records for the specified event.
    /// </summary>
    /// <param name="eventId">The unique identifier of the event for which to retrieve guest records.</param>
    /// <param name="page">The one-based page number to retrieve. Must be greater than or equal to 1.</param>
    /// <param name="pageSize">The maximum number of guest records to include in the page. Must be greater than 0.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a snapshot of guest records for the
    ///     specified page, including the total number of guests and the list of guest records for the page. If the page is
    ///     beyond the available data, the list will be empty.
    /// </returns>
    public async Task<GuestSnapshot> GetPageAsync(Guid eventId, int page, int pageSize, CancellationToken cancellationToken)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(page, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);

        var query = _dbContext.Guests.AsNoTracking()
                              .Where(g => g.Id == eventId);

        var total = await query.CountAsync(cancellationToken);
        var skip = (page - 1) * pageSize;

        var items = await query.OrderBy(g => g.Information.FirstName)
                               .Skip(skip)
                               .Take(pageSize)
                               .Select(g => new GuestRecord(g.Name, g.Description, g.ProfileUrl, g.ImageUrl))
                               .ToListAsync(cancellationToken);

        return await Task.FromResult(new GuestSnapshot(total, items));
    }

    /// <summary>
    ///     Synchronizes the event's guest list with the provided snapshot, adding, updating, or removing guests as
    ///     necessary.
    /// </summary>
    /// <remarks>
    ///     Guests are matched by name using a case-insensitive comparison. Existing guests not present in
    ///     the provided snapshot are removed. The operation is performed atomically within the database context.
    /// </remarks>
    /// <param name="eventId">The unique identifier of the event whose guest list is to be synchronized.</param>
    /// <param name="guests">
    ///     A read-only list of guest entities representing the desired state of the event's guest list. Each entity should
    ///     have a unique name (case-insensitive) within the list.
    /// </param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    ///     A SyncDelta object containing the number of guests added, updated, removed, and the total number of guests after
    ///     synchronization.
    /// </returns>
    public async Task<SyncDelta> UpsertSnapshotAsync(Guid eventId, IReadOnlyList<EventEntity> guests, CancellationToken cancellationToken)
    {
        var incomingByKey = guests.GroupBy(KeyOf, StringComparer.OrdinalIgnoreCase)
                                  .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        var existing = await _dbContext.Event.Where(g => g.Id == eventId)
                                       .ToListAsync(cancellationToken);

        var existingByKey = existing.ToDictionary(g => g.Name, StringComparer.OrdinalIgnoreCase);

        var added = 0;
        var updated = 0;

        foreach (var (key, incoming) in incomingByKey)
        {
            if (!existingByKey.TryGetValue(key, out var entity))
            {
                added++;
                _dbContext.Event.Add(new EventEntity
                {
                    Id = eventId,
                    Name = incoming.Name,
                    Description = incoming.Description,
                    ProfileUrl = incoming.ProfileUrl,
                    ImageUrl = incoming.ImageUrl
                });

                continue;
            }

            if (!Equals(entity, incoming))
            {
                updated++;
                entity.Description = incoming.Description;
                entity.ProfileUrl = incoming.ProfileUrl;
                entity.ImageUrl = incoming.ImageUrl;
            }
        }

        var incomingKeys = incomingByKey.Keys.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var toRemove = existing.Where(e => !incomingKeys.Contains(e.Name))
                               .ToList();
        var removed = toRemove.Count;

        if (removed > 0)
        {
            _dbContext.Event.RemoveRange(toRemove);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new SyncDelta(added, updated, removed, incomingByKey.Count);
    }

    /// <summary>
    ///     Retrieves the key associated with the specified guest record.
    /// </summary>
    /// <param name="guestRecord">The guest record from which to obtain the key. Cannot be null.</param>
    /// <returns>A string representing the key for the specified guest record.</returns>
    private static string KeyOf(EventEntity guestRecord)
    {
        return guestRecord.Name;
    }
}