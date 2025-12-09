using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Domain.Entities.Mcm;
using Tomeshelf.Infrastructure.Persistence;
using Tomeshelf.MCM.Api.Models;

namespace Tomeshelf.MCM.Api.Services;

/// <summary>
///     Provides operations for retrieving, updating, and deleting event configuration data.
/// </summary>
/// <remarks>
///     This service defines asynchronous methods for managing event configuration models, including fetching
///     by identifier, retrieving all configurations, updating, and deleting. Implementations should ensure thread safety
///     and proper handling of cancellation requests via the provided <see cref="CancellationToken" /> parameters.
/// </remarks>
public class EventConfigService : IEventConfigService
{
    private readonly TomeshelfMcmDbContext _dbContext;

    /// <summary>
    ///     Initializes a new instance of the EventConfigService class using the specified database context.
    /// </summary>
    /// <param name="dbContext">The database context used to access and manage event configuration data. Cannot be null.</param>
    public EventConfigService(TomeshelfMcmDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    ///     Asynchronously retrieves the configuration with the specified identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the configuration to retrieve.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains the configuration model if found;
    ///     otherwise, null.
    /// </returns>
    public async Task<EventConfigEntity> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var config = await _dbContext.EventConfigs.AsNoTracking()
                                     .Where(x => x.Id == id)
                                     .SingleOrDefaultAsync(cancellationToken);

        return config;
    }

    /// <summary>
    ///     Asynchronously deletes the event configuration with the specified identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the event configuration to delete.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the delete operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result is <see langword="true" /> if the event
    ///     configuration was found and deleted; otherwise, <see langword="false" />.
    /// </returns>
    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.EventConfigs.FindAsync(new object[] { id }, cancellationToken);
        if (entity is null)
        {
            return false;
        }

        _dbContext.EventConfigs.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    /// <summary>
    ///     Asynchronously retrieves all configuration models from the data store, ordered by name.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a read-only list of configuration
    ///     models. The list will be empty if no configurations are found.
    /// </returns>
    public async Task<IReadOnlyList<EventConfigEntity>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.EventConfigs.AsNoTracking()
                               .OrderBy(x => x.Name)
                               .ToListAsync(cancellationToken);
    }

    /// <summary>
    ///     Inserts a new configuration or updates an existing configuration in the database asynchronously.
    /// </summary>
    /// <remarks>
    ///     If a configuration with the same identifier as the provided model exists, its values are
    ///     updated; otherwise, a new configuration is created. The operation is performed within the current database
    ///     context.
    /// </remarks>
    /// <param name="model">The configuration model containing the data to insert or update. Cannot be null.</param>
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