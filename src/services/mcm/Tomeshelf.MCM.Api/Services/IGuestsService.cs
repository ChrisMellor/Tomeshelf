using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.MCM.Api.Contracts;
using Tomeshelf.MCM.Api.Models;

namespace Tomeshelf.MCM.Api.Services;

/// <summary>
///     Defines methods for retrieving and synchronizing guest data for a specified event configuration.
/// </summary>
public interface IGuestsService
{
    /// <summary>
    ///     Asynchronously retrieves a paged list of guests for the specified event configuration.
    /// </summary>
    /// <param name="model">The event configuration model that specifies the criteria for retrieving guests. Cannot be null.</param>
    /// <param name="page">The zero-based page index of the results to retrieve. Must be greater than or equal to 0.</param>
    /// <param name="pageSize">The maximum number of guests to include in a single page. Must be greater than 0.</param>
    /// <param name="includeDeleted">true to include guests that have been marked as deleted; otherwise, false.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a PagedResult of GuestDto objects for
    ///     the specified page. The result may be empty if no guests match the criteria.
    /// </returns>
    Task<PagedResult<GuestDto>> GetAsync(EventConfigModel model, int page, int pageSize, bool includeDeleted, CancellationToken cancellationToken);

    /// <summary>
    ///     Synchronizes guest data based on the specified event configuration.
    /// </summary>
    /// <param name="model">The event configuration that determines which guest data to synchronize. Cannot be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the synchronization operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a <see cref="GuestSyncResultDto" />
    ///     with details about the synchronization outcome.
    /// </returns>
    Task<GuestSyncResultDto> SyncAsync(EventConfigModel model, CancellationToken cancellationToken);
}