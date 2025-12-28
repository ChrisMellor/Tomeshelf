using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Domain.Entities.Mcm;
using Tomeshelf.Mcm.Api.Models;
using Tomeshelf.Mcm.Api.Repositories;

namespace Tomeshelf.Mcm.Api.Services;

/// <summary>
///     Provides methods for managing event configuration data, including retrieval, insertion, updating, and deletion of
///     event configurations.
/// </summary>
/// <remarks>
///     This service acts as an abstraction over the event configuration data store, enabling asynchronous
///     operations for event configuration entities. All methods support cancellation via a cancellation token. Thread
///     safety and transaction management depend on the underlying repository implementation.
/// </remarks>
public class EventService : IEventService
{
    private readonly IEventRepository _eventRepository;

    /// <summary>
    ///     Initializes a new instance of the EventService class using the specified event configuration repository.
    /// </summary>
    /// <param name="eventRepository">The repository used to access and manage event configuration data. Cannot be null.</param>
    public EventService(IEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }

    /// <summary>
    ///     Asynchronously deletes the event configuration with the specified identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the event configuration to delete.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the delete operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous delete operation. The task result is <see langword="true" /> if the event
    ///     configuration was deleted successfully; otherwise, <see langword="false" />.
    /// </returns>
    public async Task<int> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _eventRepository.DeleteAsync(id, cancellationToken);
    }

    /// <summary>
    ///     Asynchronously retrieves all event configuration entities.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a read-only list of all event
    ///     configuration entities. The list will be empty if no entities are found.
    /// </returns>
    public async Task<IReadOnlyList<EventEntity>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _eventRepository.GetAllAsync(cancellationToken);
    }

    /// <summary>
    ///     Creates a new event configuration or updates an existing one asynchronously.
    /// </summary>
    /// <param name="model">The event configuration model to insert or update. Cannot be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous upsert operation.</returns>
    public async Task UpsertAsync(EventConfigModel model, CancellationToken cancellationToken)
    {
        await _eventRepository.UpsertAsync(model, cancellationToken);
    }
}