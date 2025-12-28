using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Domain.Entities.Mcm;
using Tomeshelf.Mcm.Api.Models;

namespace Tomeshelf.Mcm.Api.Services;

/// <summary>
///     Defines a contract for managing event configuration entities, including retrieval, creation, update, and deletion
///     operations.
/// </summary>
/// <remarks>
///     All operations are asynchronous and support cancellation via a <see cref="CancellationToken" />.
///     Implementations are expected to handle entity persistence and may return <see langword="null" /> or empty
///     collections
///     when entities are not found. This interface is typically used to abstract event configuration storage and access
///     logic from application code.
/// </remarks>
public interface IEventService
{
    /// <summary>
    ///     Asynchronously deletes the entity with the specified identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity to delete.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the delete operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous delete operation. The task result is <see langword="true" /> if the entity
    ///     was successfully deleted; otherwise, <see langword="false" />.
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
    /// <param name="model">The event configuration entity to insert or update. Cannot be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous upsert operation.</returns>
    Task UpsertAsync(EventConfigModel model, CancellationToken cancellationToken);
}