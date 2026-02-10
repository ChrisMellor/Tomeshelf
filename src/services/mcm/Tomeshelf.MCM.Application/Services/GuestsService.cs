using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.MCM.Application.Abstractions.Clients;
using Tomeshelf.MCM.Application.Abstractions.Mappers;
using Tomeshelf.MCM.Application.Abstractions.Persistence;
using Tomeshelf.MCM.Application.Contracts;
using Tomeshelf.MCM.Application.Models;
using Tomeshelf.MCM.Application.Records;
using Tomeshelf.MCM.Domain.Mcm;

namespace Tomeshelf.MCM.Application.Services;

/// <summary>
///     Provides operations for retrieving and synchronizing guest information for events.
/// </summary>
/// <remarks>
///     This service acts as a bridge between the external guests management system and the local data
///     repository, enabling retrieval of paged guest lists and synchronization of guest data for specific events. It is
///     intended for internal use within the application and is not thread-safe.
/// </remarks>
public sealed class GuestsService : IGuestsService
{
    private readonly IMcmGuestsClient _client;
    private readonly IGuestMapper _mapper;
    private readonly IGuestsRepository _repository;

    /// <summary>
    ///     Initializes a new instance of the GuestsService class with the specified client, mapper, and repository
    ///     dependencies.
    /// </summary>
    /// <param name="client">The client used to communicate with the external guests management system. Cannot be null.</param>
    /// <param name="mapper">The mapper responsible for converting between domain and data transfer objects. Cannot be null.</param>
    /// <param name="repository">The repository used for accessing and persisting guest data. Cannot be null.</param>
    public GuestsService(IMcmGuestsClient client, IGuestMapper mapper, IGuestsRepository repository)
    {
        _client = client;
        _mapper = mapper;
        _repository = repository;
    }

    /// <summary>
    ///     Retrieves a paged list of guests for the specified event configuration.
    /// </summary>
    /// <param name="model">
    ///     The event configuration model that identifies the event for which to retrieve guests. Cannot be
    ///     null.
    /// </param>
    /// <param name="page">The zero-based page index of the results to retrieve. Must be greater than or equal to 0.</param>
    /// <param name="pageSize">The maximum number of guests to include in a single page. Must be greater than 0.</param>
    /// <param name="includeDeleted">true to include guests that have been marked as deleted; otherwise, false.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a paged result of guest data transfer
    ///     objects for the specified event.
    /// </returns>
    public async Task<PagedResult<GuestDto>> GetAsync(EventConfigModel model, int page, int pageSize, bool includeDeleted, CancellationToken cancellationToken)
    {
        var snapshot = await _repository.GetPageAsync(model.Id, page, pageSize, includeDeleted, cancellationToken);

        var items = snapshot.Items
                            .Select(x => new GuestDto(x.Id, x.Name, x.Description, x.ProfileUrl, x.ImageUrl, x.AddedAt, x.RemovedAt, x.IsDeleted))
                            .ToList();

        var pagedResult = new PagedResult<GuestDto>(snapshot.Total, items, page, pageSize);

        return pagedResult;
    }

    /// <summary>
    ///     Synchronizes the guest list for the specified event by fetching the latest guest data and updating the local
    ///     event record accordingly.
    /// </summary>
    /// <remarks>
    ///     This method updates the local event's guest list to match the latest data from the external
    ///     source. Guests not present in the latest data are marked as deleted. The operation is performed asynchronously
    ///     and saves changes to the underlying data store.
    /// </remarks>
    /// <param name="model">The event configuration model that identifies the event to synchronize. Must not be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>
    ///     A <see cref="GuestSyncResultDto" /> containing the results of the synchronization, including the number of guests
    ///     added, updated, removed, and the total active guests. Returns null if the specified event does not exist.
    /// </returns>
    public async Task<GuestSyncResultDto> SyncAsync(EventConfigModel model, CancellationToken cancellationToken)
    {
        var eventEntity = await _repository.GetEventWithGuestsAsync(model.Id, cancellationToken);
        if (eventEntity is null)
        {
            return null;
        }

        var fetched = await _client.FetchGuestsAsync(model.Id, cancellationToken);

        var existingByKey = new Dictionary<string, GuestEntity>(StringComparer.OrdinalIgnoreCase);
        foreach (var guest in eventEntity.Guests)
        {
            var key = _mapper.GetGuestKey(guest);
            if (!string.IsNullOrWhiteSpace(key))
            {
                existingByKey.TryAdd(key, guest);
            }
        }

        var seenKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var added = 0;
        var updated = 0;
        var removed = 0;

        foreach (var record in fetched ?? Array.Empty<GuestRecord>())
        {
            if (record is null)
            {
                continue;
            }

            var source = MapRecordToGuest(record);
            var key = _mapper.GetGuestKey(source);
            if (string.IsNullOrWhiteSpace(key))
            {
                continue;
            }

            seenKeys.Add(key);

            if (existingByKey.TryGetValue(key, out var existing))
            {
                if (_mapper.UpdateGuest(existing, source))
                {
                    updated++;
                }

                continue;
            }

            var inserted = _mapper.CloneForEvent(model.Id, source);
            inserted.AddedAt = DateTimeOffset.UtcNow;
            inserted.RemovedAt = null;
            eventEntity.Guests.Add(inserted);
            _repository.AddGuest(inserted);
            existingByKey.Add(key, inserted);
            added++;
        }

        foreach (var guest in eventEntity.Guests)
        {
            var key = _mapper.GetGuestKey(guest);
            if (string.IsNullOrWhiteSpace(key))
            {
                continue;
            }

            if (!seenKeys.Contains(key) && !guest.IsDeleted)
            {
                guest.IsDeleted = true;
                guest.RemovedAt = DateTimeOffset.UtcNow;
                removed++;
            }
        }

        if ((added > 0) || (updated > 0) || (removed > 0))
        {
            eventEntity.UpdatedAt = DateTimeOffset.UtcNow;
        }

        await _repository.SaveChangesAsync(cancellationToken);

        var total = eventEntity.Guests.Count(g => !g.IsDeleted);

        return new GuestSyncResultDto("Succeeded", added, updated, removed, total, DateTimeOffset.UtcNow);
    }

    /// <summary>
    ///     Maps the record to guest.
    /// </summary>
    /// <param name="record">The record.</param>
    /// <returns>The result of the operation.</returns>
    internal static GuestEntity MapRecordToGuest(GuestRecord record)
    {
        var (firstName, lastName) = SplitName(record.Name);
        var profileUrl = record.ProfileUrl;

        var information = new GuestInfoEntity
        {
            FirstName = firstName,
            LastName = lastName,
            Bio = record.Description,
            ImageUrl = record.ImageUrl,
            Socials = string.IsNullOrWhiteSpace(profileUrl)
                ? null
                : new GuestSocial { Imdb = profileUrl }
        };

        return new GuestEntity { Information = information };
    }

    /// <summary>
    ///     Splits the name.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <returns>The result of the operation.</returns>
    internal static (string FirstName, string LastName) SplitName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return (string.Empty, string.Empty);
        }

        var trimmed = name.Trim();
        var lastSpace = trimmed.LastIndexOf(' ');

        if (lastSpace <= 0)
        {
            return (trimmed, string.Empty);
        }

        var firstName = trimmed[..lastSpace]
           .Trim();
        var lastName = trimmed[(lastSpace + 1)..]
           .Trim();

        return (firstName, lastName);
    }
}