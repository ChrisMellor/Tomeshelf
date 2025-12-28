using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Domain.Entities.Mcm;
using Tomeshelf.Infrastructure.Persistence;
using Tomeshelf.Mcm.Api.Models;

namespace Tomeshelf.Mcm.Api.Repositories;

/// <summary>
///     Provides methods for managing event configuration entities in the data store, including retrieval, insertion,
///     updating, and deletion operations.
/// </summary>
/// <remarks>
///     This repository encapsulates data access logic for event configurations, enabling asynchronous CRUD
///     operations. It is intended to be used with dependency injection and requires a valid database context for
///     operation.
/// </remarks>
public class EventRepository : IEventRepository
{
    private readonly TomeshelfMcmDbContext _dbContext;

    /// <summary>
    ///     Initializes a new instance of the EventRepository class using the specified database context.
    /// </summary>
    /// <param name="dbContext">The database context to be used for data access operations. Cannot be null.</param>
    public EventRepository(TomeshelfMcmDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    ///     Asynchronously retrieves all event configuration entities, ordered by name.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    ///     A read-only list of all event configuration entities, ordered alphabetically by name. The list will be empty if
    ///     no entities are found.
    /// </returns>
    public async Task<IReadOnlyList<EventEntity>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Events.AsNoTracking()
                               .OrderBy(x => x.Name)
                               .ToListAsync(cancellationToken);
    }

    /// <summary>
    ///     Inserts a new event or updates an existing event in the database asynchronously based on the specified event
    ///     configuration.
    /// </summary>
    /// <remarks>
    ///     If an event with the specified identifier exists, its name and update timestamp are modified.
    ///     Otherwise, a new event is created. This method saves changes to the database immediately.
    /// </remarks>
    /// <param name="model">The event configuration model containing the event data to insert or update. Must not be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains the number of state entries written
    ///     to the database.
    /// </returns>
    public async Task<int> UpsertAsync(EventConfigModel model, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Events.FindAsync([model.Id], cancellationToken);

        if (entity is null)
        {
            entity = new EventEntity
            {
                Id = model.Id,
                Name = model.Name,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            _dbContext.Events.Add(entity);
        }
        else
        {
            entity.Name = model.Name;
            entity.UpdatedAt = DateTimeOffset.UtcNow;
        }

        return await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    ///     Asynchronously deletes the event with the specified identifier from the data store.
    /// </summary>
    /// <param name="id">The unique identifier of the event to delete.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the delete operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result is <see langword="true" /> if the event was
    ///     found
    ///     and deleted; otherwise, <see langword="false" />.
    /// </returns>
    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Events.FindAsync([id], cancellationToken);
        if (entity is null)
        {
            return false;
        }

        _dbContext.Events.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}