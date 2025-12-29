using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Mcm.Api.Contracts;
using Tomeshelf.Mcm.Api.Models;

namespace Tomeshelf.Mcm.Api.Services;

/// <summary>
///     Defines methods for retrieving and synchronizing guest data for a specified event configuration.
/// </summary>
public interface IGuestsService
{
    /// <summary>
    ///     Asynchronously retrieves a paged list of guests for the specified event configuration.
    /// </summary>
    /// <param name="model">The event configuration used to filter or identify the guests to retrieve. Cannot be null.</param>
    /// <param name="page">The zero-based index of the page to retrieve. Must be greater than or equal to 0.</param>
    /// <param name="pageSize">The maximum number of guests to include in a single page. Must be greater than 0.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a
    ///     <see
    ///         cref="PagedResult{GuestDto}" />
    ///     with the guests for the specified page. The result may be empty if no guests
    ///     match the criteria.
    /// </returns>
    Task<PagedResult<GuestDto>> GetAsync(EventConfigModel model, int page, int pageSize, CancellationToken cancellationToken);

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