using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Domain.Entities.Mcm;
using Tomeshelf.Mcm.Api.Models;

namespace Tomeshelf.Mcm.Api.Repositories;

/// <summary>
///     Defines a contract for managing event configuration entities in a data store.
/// </summary>
/// <remarks>
///     Implementations of this interface provide asynchronous methods for retrieving, creating, updating,
///     and deleting event configuration entities. All operations support cancellation via a cancellation token. This
///     interface is intended to abstract the persistence mechanism for event configurations, enabling flexible storage
///     strategies.
/// </remarks>
public interface IEventRepository
{
    /// <summary>
    ///     Asynchronously deletes the entity with the specified identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity to delete.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the delete operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous delete operation. The task result is <see langword="true" /> if the
    ///     entity was successfully deleted; otherwise, <see langword="false" />.
    /// </returns>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    ///     Asynchronously retrieves all event configuration entities.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a read-only list of all event
    ///     configuration entities. The list will be empty if no entities are found.
    /// </returns>
    Task<IReadOnlyList<EventEntity>> GetAllAsync(CancellationToken cancellationToken);

    /// <summary>
    ///     Creates a new event configuration or updates an existing one asynchronously.
    /// </summary>
    /// <param name="model">The event configuration to create or update. Cannot be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous upsert operation.</returns>
    Task UpsertAsync(EventConfigModel model, CancellationToken cancellationToken);
}