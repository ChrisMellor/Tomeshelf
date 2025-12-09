using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.MCM.Api.Contracts;
using Tomeshelf.MCM.Api.Models;
using Tomeshelf.MCM.Api.Repositories;

namespace Tomeshelf.MCM.Api.Services;

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
    ///     Synchronizes guest data for the specified event and returns the result of the synchronization operation.
    /// </summary>
    /// <param name="model">The event configuration containing the event identifier and related settings. Cannot be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the synchronization operation.</param>
    /// <returns>
    ///     A <see cref="GuestSyncResultDto" /> containing the outcome of the synchronization, including counts of added,
    ///     updated, and removed guests.
    /// </returns>
    public async Task<GuestSyncResultDto> SyncAsync(EventConfigModel model, CancellationToken cancellationToken)
    {
        var fetched = await _client.FetchGuestsAsync(model.Id, cancellationToken);
        var delta = await _repository.UpsertSnapshotAsync(model.Id, fetched, cancellationToken);

        var guestSyncResult = new GuestSyncResultDto(model.Name, "Succeeded", delta.Added, delta.Updated, delta.Removed, delta.Total, DateTimeOffset.UtcNow);

        return guestSyncResult;
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

        var pagedResult = new PagedResult<GuestDto>(page, pageSize, snapshot.Total, items);

        return pagedResult;
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
}