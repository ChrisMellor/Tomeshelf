using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Domain.Entities.Mcm;
using Tomeshelf.Mcm.Api.Models;

namespace Tomeshelf.Mcm.Api.Repositories;

/// <summary>
///     Defines methods for managing event configuration entities in a data store.
/// </summary>
/// <remarks>
///     Implementations of this interface provide asynchronous operations for retrieving, inserting,
///     updating, and deleting event configuration entities. All methods support cancellation via a cancellation token.
///     This
///     interface is intended to abstract the persistence mechanism for event configurations, enabling flexible storage
///     implementations.
/// </remarks>
public interface IEventRepository
{
    /// <summary>
    ///     Asynchronously deletes the entity with the specified identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity to delete. Cannot be null or empty.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the delete operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous delete operation. The task result is <see langword="true" /> if the
    ///     entity was successfully deleted; otherwise, <see langword="false" />.
    /// </returns>
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken);

    /// <summary>
    ///     Asynchronously retrieves all event entities.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a read-only list of all event
    ///     entities. The list will be empty if no events are found.
    /// </returns>
    Task<IReadOnlyList<EventEntity>> GetAllAsync(CancellationToken cancellationToken);

    /// <summary>
    ///     Inserts a new event configuration or updates an existing one asynchronously.
    /// </summary>
    /// <param name="model">The event configuration to insert or update. Cannot be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains the number of records affected by
    ///     the upsert operation.
    /// </returns>
    Task<int> UpsertAsync(EventConfigModel model, CancellationToken cancellationToken);
}