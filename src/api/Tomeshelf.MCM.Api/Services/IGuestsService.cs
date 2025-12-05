using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.MCM.Api.Contracts;
using Tomeshelf.MCM.Api.Enums;

namespace Tomeshelf.MCM.Api.Services;

/// <summary>
///     Defines operations for synchronizing, retrieving, and deleting guest data for a specified city.
/// </summary>
/// <remarks>
///     Implementations of this interface should ensure thread safety if accessed concurrently. Methods are
///     asynchronous and support cancellation via the provided <see cref="CancellationToken" /> parameter.
/// </remarks>
public interface IGuestsService
{
    /// <summary>
    ///     Synchronizes guest data for the specified city asynchronously.
    /// </summary>
    /// <param name="city">The city for which guest data will be synchronized. Cannot be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the synchronization operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a <see cref="GuestSyncResultDto" />
    ///     with details of the synchronization outcome.
    /// </returns>
    Task<GuestSyncResultDto> SyncAsync(City city, CancellationToken cancellationToken);

    /// <summary>
    ///     Asynchronously retrieves a paged list of guests for the specified city.
    /// </summary>
    /// <param name="city">The city for which to retrieve guest information. Cannot be null.</param>
    /// <param name="page">
    ///     The zero-based page index indicating which page of results to retrieve. Must be greater than or
    ///     equal to 0.
    /// </param>
    /// <param name="pageSize">The maximum number of guests to include in a single page of results. Must be greater than 0.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a
    ///     <see
    ///         cref="PagedResult{GuestDto}" />
    ///     with the guests for the specified city and page.
    /// </returns>
    Task<PagedResult<GuestDto>> GetAsync(City city, int page, int pageSize, CancellationToken cancellationToken);

    /// <summary>
    ///     Asynchronously deletes the specified city from the data store.
    /// </summary>
    /// <param name="city">The city to delete. Cannot be null.</param>
    /// <param name="cancellationToken">
    ///     A token to monitor for cancellation requests. The operation is canceled if the token is
    ///     triggered.
    /// </param>
    /// <returns>A task that represents the asynchronous delete operation.</returns>
    Task DeleteAsync(City city, CancellationToken cancellationToken);
}