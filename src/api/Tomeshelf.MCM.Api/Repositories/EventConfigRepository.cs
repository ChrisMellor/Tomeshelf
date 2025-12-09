using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Tomeshelf.Domain.Entities.Mcm;
using Tomeshelf.Infrastructure.Persistence;
using Tomeshelf.MCM.Api.Models;

namespace Tomeshelf.MCM.Api.Repositories;

/// <summary>
///     Provides methods for managing event configuration entities in the data store, including retrieval, insertion,
///     updating, and deletion operations.
/// </summary>
/// <remarks>
///     This repository encapsulates data access logic for event configurations, enabling asynchronous CRUD
///     operations. It is intended to be used with dependency injection and requires a valid database context for
///     operation.
/// </remarks>
public class EventConfigRepository : IEventConfigRepository
{
    private readonly TomeshelfMcmDbContext _dbContext;

    /// <summary>
    ///     Initializes a new instance of the EventConfigRepository class using the specified database context.
    /// </summary>
    /// <param name="dbContext">The database context to be used for data access operations. Cannot be null.</param>
    public EventConfigRepository(TomeshelfMcmDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    ///     Asynchronously deletes the event configuration with the specified identifier, if it exists.
    /// </summary>
    /// <param name="id">The unique identifier of the event configuration to delete.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the delete operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result is <see langword="true" /> if the event
    ///     configuration was found and deleted; otherwise, <see langword="false" />.
    /// </returns>
    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.EventConfigs.FindAsync([id], cancellationToken);
        if (entity is null)
        {
            return false;
        }

        _dbContext.EventConfigs.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    /// <summary>
    ///     Asynchronously retrieves all event configuration entities, ordered by name.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    ///     A read-only list of all event configuration entities, ordered alphabetically by name. The list will be empty if
    ///     no entities are found.
    /// </returns>
    public async Task<IReadOnlyList<EventConfigEntity>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.EventConfigs.AsNoTracking()
                               .OrderBy(x => x.Name)
                               .ToListAsync(cancellationToken);
    }

    /// <summary>
    ///     Inserts a new event configuration or updates an existing one asynchronously based on the specified model.
    /// </summary>
    /// <remarks>
    ///     If an event configuration with the same identifier as the model exists, its properties are
    ///     updated; otherwise, a new configuration is created. The operation is performed within the current database
    ///     context.
    /// </remarks>
    /// <param name="model">The event configuration data to insert or update. Must not be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous upsert operation.</returns>
    public async Task UpsertAsync(EventConfigModel model, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.EventConfigs.FindAsync([model.Id], cancellationToken);

        if (entity is null)
        {
            entity = new EventConfigEntity
            {
                    Id = model.Id,
                    Name = model.Name,
                    UpdatedAt = DateTimeOffset.UtcNow
            };

            _dbContext.EventConfigs.Add(entity);
        }
        else
        {
            entity.Name = model.Name;
            entity.UpdatedAt = DateTimeOffset.UtcNow;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}