using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.MCM.Api.Clients;
using Tomeshelf.MCM.Api.Contracts;
using Tomeshelf.MCM.Api.Enums;
using Tomeshelf.MCM.Api.Repositories;

namespace Tomeshelf.MCM.Api.Services;

/// <summary>
///     Provides operations for synchronizing, retrieving, and deleting guest data for a specified city.
/// </summary>
/// <remarks>
///     This service coordinates guest data between an external source and the local repository. All
///     operations are asynchronous and require a valid city context. Thread safety is ensured for concurrent calls to
///     service methods.
/// </remarks>
internal sealed class GuestsService : IGuestsService
{
    private readonly IMcmGuestsClient _client;
    private readonly IGuestsRepository _repository;

    /// <summary>
    ///     Initializes a new instance of the GuestsService class using the specified guests client and repository.
    /// </summary>
    /// <param name="client">The client used to interact with the external guests management system. Cannot be null.</param>
    /// <param name="repository">The repository used for local guests data storage and retrieval. Cannot be null.</param>
    public GuestsService(IMcmGuestsClient client, IGuestsRepository repository)
    {
        _client = client;
        _repository = repository;
    }

    /// <summary>
    ///     Synchronizes guest data for the specified city by fetching the latest information and updating the local
    ///     snapshot.
    /// </summary>
    /// <remarks>
    ///     This method performs both data retrieval and update operations. The returned result reflects
    ///     the changes made during synchronization. The operation is asynchronous and may be cancelled via the provided
    ///     token.
    /// </remarks>
    /// <param name="city">The city for which guest data will be synchronized. Cannot be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the synchronization operation.</param>
    /// <returns>
    ///     A <see cref="GuestSyncResultDto" /> containing the results of the synchronization, including counts of added,
    ///     updated, and removed guests.
    /// </returns>
    public async Task<GuestSyncResultDto> SyncAsync(City city, CancellationToken cancellationToken)
    {
        var fetched = await _client.FetchGuestsAsync(city, cancellationToken);
        var delta = await _repository.UpsertSnapshotAsync(city, fetched, cancellationToken);

        var guestSyncResult = new GuestSyncResultDto(city, "Succeeded", delta.Added, delta.Updated, delta.Removed, delta.Total, DateTimeOffset.UtcNow);

        return guestSyncResult;
    }

    /// <summary>
    ///     Asynchronously retrieves a paged list of guests for the specified city.
    /// </summary>
    /// <remarks>
    ///     If the specified page exceeds the available data, the returned list of guests will be empty.
    ///     This method does not filter or sort guests beyond paging.
    /// </remarks>
    /// <param name="city">The city for which to retrieve guest information.</param>
    /// <param name="page">
    ///     The zero-based page index indicating which page of results to return. Must be greater than or equal
    ///     to 0.
    /// </param>
    /// <param name="pageSize">The maximum number of guests to include in a single page. Must be greater than 0.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a
    ///     <see
    ///         cref="PagedResult{GuestDto}" />
    ///     with the guests for the specified city and page.
    /// </returns>
    public async Task<PagedResult<GuestDto>> GetAsync(City city, int page, int pageSize, CancellationToken cancellationToken)
    {
        var snapshot = await _repository.GetPageAsync(city, page, pageSize, cancellationToken);

        var items = snapshot.Items.Select(x => new GuestDto(x.Name, x.Description, x.ProfileUrl, x.ImageUrl))
                            .ToList();

        var pagedResult = new PagedResult<GuestDto>(page, pageSize, snapshot.Total, items);

        return pagedResult;
    }

    /// <summary>
    ///     Asynchronously deletes all records associated with the specified city.
    /// </summary>
    /// <param name="city">The city for which all associated records will be deleted. Cannot be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the delete operation.</param>
    /// <returns>A task that represents the asynchronous delete operation.</returns>
    public async Task DeleteAsync(City city, CancellationToken cancellationToken)
    {
        await _repository.DeleteAllAsync(city, cancellationToken);
    }
}