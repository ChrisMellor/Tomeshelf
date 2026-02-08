using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Tomeshelf.MCM.Application.Abstractions.Persistence;
using Tomeshelf.MCM.Application.Records;
using Tomeshelf.MCM.Domain.Mcm;

namespace Tomeshelf.MCM.Infrastructure.Repositories;

/// <summary>
///     Provides data access methods for retrieving and managing guest and event information in the Tomeshelf system.
/// </summary>
/// <remarks>
///     This repository encapsulates operations related to guests and their associated events, including
///     retrieval of event details with guests, paginated guest listings, and saving changes to the database. All
///     operations
///     are performed asynchronously and require a valid database context. This class is not thread-safe; use a separate
///     instance per operation scope as recommended for Entity Framework Core repositories.
/// </remarks>
public class GuestsRepository : IGuestsRepository
{
    private readonly TomeshelfMcmDbContext _dbContext;

    /// <summary>
    ///     Initializes a new instance of the GuestsRepository class using the specified database context.
    /// </summary>
    /// <param name="dbContext">The database context to be used for data access operations. Cannot be null.</param>
    public GuestsRepository(TomeshelfMcmDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    ///     Adds a guest entity to the data context for tracking and persistence.
    /// </summary>
    /// <param name="guest">The guest entity to add to the context. Cannot be null.</param>
    public void AddGuest(GuestEntity guest)
    {
        _dbContext.Guests.Add(guest);
    }

    /// <summary>
    ///     Asynchronously retrieves an event by its identifier, including its associated guests and their related information
    ///     and socials.
    /// </summary>
    /// <param name="eventId">The unique identifier of the event to retrieve. Cannot be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains the event entity with its guests and
    ///     their related information and socials, or null if no event with the specified identifier is found.
    /// </returns>
    public async Task<EventEntity> GetEventWithGuestsAsync(string eventId, CancellationToken cancellationToken)
    {
        return await _dbContext.Events
                               .Include(e => e.Guests)
                               .ThenInclude(g => g.Information)
                               .ThenInclude(i => i.Socials)
                               .SingleOrDefaultAsync(e => e.Id == eventId, cancellationToken);
    }

    /// <summary>
    ///     Asynchronously retrieves a paged list of guests for the specified event.
    /// </summary>
    /// <param name="eventId">The unique identifier of the event for which to retrieve guests.</param>
    /// <param name="page">The one-based page number to retrieve. Must be greater than or equal to 1.</param>
    /// <param name="pageSize">The number of guests to include in each page. Must be greater than or equal to 1.</param>
    /// <param name="includeDeleted">true to include guests marked as deleted; otherwise, false.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a GuestSnapshot with the total number
    ///     of
    ///     guests and the list of guests for the specified page.
    /// </returns>
    public async Task<GuestSnapshot> GetPageAsync(string eventId, int page, int pageSize, bool includeDeleted, CancellationToken cancellationToken)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(page, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);

        var query = _dbContext.Guests
                              .AsNoTracking()
                              .Where(g => g.EventId == eventId);

        if (!includeDeleted)
        {
            query = query.Where(g => !g.IsDeleted);
        }

        var total = await query.CountAsync(cancellationToken);
        var skip = (page - 1) * pageSize;

        var items = await query.OrderBy(g => g.Information != null
                                            ? g.Information.FirstName
                                            : string.Empty)
                               .ThenBy(g => g.Information != null
                                           ? g.Information.LastName
                                           : string.Empty)
                               .Skip(skip)
                               .Take(pageSize)
                               .Select(g => new GuestListItem(g.Id, (g.Information != null
                                                                        ? g.Information.FirstName
                                                                        : string.Empty) +
                                                                    " " +
                                                                    (g.Information != null
                                                                        ? g.Information.LastName
                                                                        : string.Empty), g.Information != null
                                                                  ? g.Information.Bio ?? string.Empty
                                                                  : string.Empty, (g.Information != null) && (g.Information.Socials != null)
                                                                  ? g.Information.Socials.Imdb ?? string.Empty
                                                                  : string.Empty, g.Information != null
                                                                  ? g.Information.ImageUrl ?? string.Empty
                                                                  : string.Empty, g.AddedAt, g.RemovedAt, g.IsDeleted))
                               .ToListAsync(cancellationToken);

        return new GuestSnapshot(total, items);
    }

    /// <summary>
    ///     Asynchronously saves all changes made in the current context to the underlying database.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the save operation.</param>
    /// <returns>A task that represents the asynchronous save operation.</returns>
    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}