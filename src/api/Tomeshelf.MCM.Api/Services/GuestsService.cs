using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Mcm.Api.Clients;
using Tomeshelf.Mcm.Api.Contracts;
using Tomeshelf.Mcm.Api.Models;
using Tomeshelf.Mcm.Api.Repositories;

namespace Tomeshelf.Mcm.Api.Services;

/// <summary>
///     Provides operations for synchronizing, retrieving, and deleting guest data for events using an external guests
///     management system and a local repository.
/// </summary>
internal sealed class GuestsService : IGuestsService
{
    private readonly IMcmGuestsClient _client;
    private readonly IGuestsRepository _repository;

    /// <summary>
    ///     Initializes a new instance of the GuestsService class with the specified client, repository, and logger.
    /// </summary>
    /// <param name="client">The client used to interact with the external guests management system.</param>
    /// <param name="repository">The repository used for accessing and persisting guest data.</param>
    public GuestsService(IMcmGuestsClient client, IGuestsRepository repository)
    {
        _client = client;
        _repository = repository;
    }

    /// <summary>
    ///     Asynchronously deletes all event configuration data associated with the specified model.
    /// </summary>
    /// <param name="model">The event configuration model whose associated data will be deleted. Cannot be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the delete operation.</param>
    /// <returns>A task that represents the asynchronous delete operation.</returns>
    public async Task DeleteAsync(EventConfigModel model, CancellationToken cancellationToken)
    {
        await _repository.DeleteAllAsync(model.Id, cancellationToken);
    }

    /// <summary>
    ///     Retrieves a paged list of guests for the specified event configuration.
    /// </summary>
    /// <param name="model">
    ///     The event configuration model that identifies the event for which to retrieve guests. Cannot be
    ///     null.
    /// </param>
    /// <param name="page">The one-based page index of the results to retrieve. Must be greater than or equal to 1.</param>
    /// <param name="pageSize">The maximum number of guests to include in a single page. Must be greater than 0.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a paged result of guest data transfer
    ///     objects for the specified event.
    /// </returns>
    public async Task<PagedResult<GuestDto>> GetAsync(EventConfigModel model, int page, int pageSize, CancellationToken cancellationToken)
    {
        var snapshot = await _repository.GetPageAsync(model.Id, page, pageSize, cancellationToken);

        var items = snapshot.Items.Select(x => new GuestDto(x.Name, x.Description, x.ProfileUrl, x.ImageUrl))
                            .ToList();

        var pagedResult = new PagedResult<GuestDto>(snapshot.Total, items, page, pageSize);

        return pagedResult;
    }

    /// <summary>
    ///     Synchronizes the guest list for the specified event by fetching the latest guest data and updating the local
    ///     snapshot asynchronously.
    /// </summary>
    /// <remarks>
    ///     This method fetches the current guest list from the external source and updates the local
    ///     repository to reflect any changes. The returned result provides details about the synchronization, such as the
    ///     number of guests added, updated, or removed. If the operation is canceled via the provided token, the task is
    ///     canceled.
    /// </remarks>
    /// <param name="eventId">The unique identifier of the event whose guest list is to be synchronized.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a <see cref="GuestSyncResultDto" />
    ///     describing the outcome of the synchronization, including counts of added, updated, and removed guests.
    /// </returns>
    public async Task<GuestSyncResultDto> SyncAsync(Guid eventId, CancellationToken cancellationToken)
    {
        var fetched = await _client.FetchGuestsAsync(eventId, cancellationToken);
        var delta = await _repository.UpsertSnapshotAsync(eventId, fetched, cancellationToken);

        var guestSyncResult = new GuestSyncResultDto("Succeeded", delta.Added, delta.Updated, delta.Removed, delta.Total, DateTimeOffset.UtcNow);

        return guestSyncResult;
    }
}