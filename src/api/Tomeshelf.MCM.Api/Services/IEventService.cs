using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Domain.Shared.Entities.Mcm;
using Tomeshelf.Mcm.Api.Models;

namespace Tomeshelf.Mcm.Api.Services;

/// <summary>
///     Defines the contract for managing event configuration entities, including retrieval, creation, update, and deletion
///     operations.
/// </summary>
/// <remarks>
///     Implementations of this interface provide asynchronous methods for working with event configuration
///     data. All operations support cancellation via a cancellation token. Methods may throw exceptions for invalid
///     arguments or if the operation cannot be completed due to the current state of the data store.
/// </remarks>
public interface IEventService
{
    /// <summary>
    ///     Asynchronously deletes the entity with the specified identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity to delete. Cannot be null or empty.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the delete operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous delete operation. The task result is <see langword="true" /> if the entity
    ///     was successfully deleted; otherwise, <see langword="false" />.
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
    ///     Creates a new event configuration or updates an existing one asynchronously.
    /// </summary>
    /// <param name="model">The event configuration to create or update. Cannot be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous upsert operation.</returns>
    Task UpsertAsync(EventConfigModel model, CancellationToken cancellationToken);
}