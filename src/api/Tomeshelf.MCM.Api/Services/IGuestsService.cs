using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.MCM.Api.Contracts;
using Tomeshelf.MCM.Api.Models;

namespace Tomeshelf.MCM.Api.Services;

/// <summary>
///     Defines methods for synchronizing, retrieving, and deleting guest data associated with a specific event
///     configuration.
/// </summary>
/// <remarks>
///     Implementations of this interface provide asynchronous operations for managing guest information in
///     the context of event configurations. All methods support cancellation via a <see cref="CancellationToken" /> to
///     allow
///     responsive and robust client applications.
/// </remarks>
public interface IGuestsService
{
    /// <summary>
    ///     Synchronizes guest data for the specified event configuration asynchronously.
    /// </summary>
    /// <param name="model">
    ///     The event configuration model that defines the parameters and settings for the guest synchronization operation.
    ///     Cannot be null.
    /// </param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a <see cref="GuestSyncResultDto" />
    ///     with the outcome of the guest synchronization.
    /// </returns>
    Task<GuestSyncResultDto> SyncAsync(EventConfigModel model, CancellationToken cancellationToken);

    /// <summary>
    ///     Asynchronously retrieves a paged list of guests for the specified event configuration.
    /// </summary>
    /// <param name="model">The event configuration model that specifies the criteria for selecting guests. Cannot be null.</param>
    /// <param name="page">The zero-based index of the page to retrieve. Must be greater than or equal to 0.</param>
    /// <param name="pageSize">The maximum number of guests to include in a single page. Must be greater than 0.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a paged result of guest data
    ///     transfer objects. The result will be empty if no guests match the specified criteria.
    /// </returns>
    Task<PagedResult<GuestDto>> GetAsync(EventConfigModel model, int page, int pageSize, CancellationToken cancellationToken);

    /// <summary>
    ///     Asynchronously deletes the specified event configuration.
    /// </summary>
    /// <param name="model">The event configuration to delete. Cannot be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the delete operation.</param>
    /// <returns>A task that represents the asynchronous delete operation.</returns>
    Task DeleteAsync(EventConfigModel model, CancellationToken cancellationToken);
}