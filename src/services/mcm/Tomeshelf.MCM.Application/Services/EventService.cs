using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.MCM.Application.Abstractions.Persistence;
using Tomeshelf.MCM.Application.Models;
using Tomeshelf.MCM.Domain.Mcm;

namespace Tomeshelf.MCM.Application.Services;

/// <summary>
///     Provides methods for managing event configuration entities, including retrieval, creation, update, and deletion
///     operations.
/// </summary>
/// <remarks>
///     The EventService class serves as the primary entry point for interacting with event configuration
///     data. It abstracts the underlying data repository and exposes asynchronous methods for common event management
///     tasks. This class is typically used in application layers that require access to event configuration
///     functionality.
/// </remarks>
public class EventService : IEventService
{
    private readonly IEventRepository _eventRepository;

    /// <summary>
    ///     Initializes a new instance of the EventService class using the specified event repository.
    /// </summary>
    /// <param name="eventRepository">The repository used to access and manage event data. Cannot be null.</param>
    public EventService(IEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }

    /// <summary>
    ///     Asynchronously deletes the event with the specified identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the event to delete. Cannot be null or empty.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the delete operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result is <see langword="true" /> if the event was
    ///     successfully deleted; otherwise, <see langword="false" />.
    /// </returns>
    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken)
    {
        return await _eventRepository.DeleteAsync(id, cancellationToken);
    }

    /// <summary>
    ///     Asynchronously retrieves all event entities from the data store.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a read-only list of all event
    ///     entities.
    /// </returns>
    public async Task<IReadOnlyList<EventConfigModel>> GetAllAsync(CancellationToken cancellationToken)
    {
        var entities = await _eventRepository.GetAllAsync(cancellationToken);

        return entities
            .Select(entity => new EventConfigModel
            {
                Id = entity.Id,
                Name = entity.Name
            })
            .ToList();
    }

    /// <summary>
    ///     Inserts a new event configuration or updates an existing one asynchronously.
    /// </summary>
    /// <param name="model">The event configuration model to insert or update. Cannot be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous upsert operation.</returns>
    public async Task UpsertAsync(EventConfigModel model, CancellationToken cancellationToken)
    {
        await _eventRepository.UpsertAsync(model, cancellationToken);
    }
}