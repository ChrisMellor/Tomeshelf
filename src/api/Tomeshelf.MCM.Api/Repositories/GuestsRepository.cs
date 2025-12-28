using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Domain.Entities.Mcm;
using Tomeshelf.Infrastructure.Persistence;
using Tomeshelf.Mcm.Api.Records;

namespace Tomeshelf.Mcm.Api.Repositories;

/// <summary>
///     Provides methods for managing guest records associated with events, including retrieval, synchronization, and
///     deletion operations.
/// </summary>
/// <remarks>
///     This repository encapsulates data access logic for guest-related operations within the event context.
///     All methods are asynchronous and require a valid database context. Thread safety is determined by the underlying
///     database context implementation.
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
    ///     Asynchronously deletes all event records with the specified event identifier.
    /// </summary>
    /// <param name="eventId">
    ///     The unique identifier of the event to delete. Only records matching this identifier will be
    ///     removed.
    /// </param>
    /// <param name="cancellationToken">A token that can be used to cancel the delete operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the number of records deleted.</returns>
    public async Task<int> DeleteAllAsync(Guid eventId, CancellationToken cancellationToken)
    {
        return await _dbContext.Events.Where(x => x.Id == eventId)
                               .ExecuteDeleteAsync(cancellationToken);
    }

    /// <summary>
    ///     Asynchronously retrieves the event with the specified identifier, including its guests and related information.
    /// </summary>
    /// <remarks>
    ///     The returned event includes its guests, each guest's information, and their associated social
    ///     profiles. If no event with the specified identifier exists, the result is null.
    /// </remarks>
    /// <param name="eventId">The unique identifier of the event to retrieve.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains the event entity with its guests and
    ///     their associated information if found; otherwise, null.
    /// </returns>
    public async Task<EventEntity> GetGuestsByIdAsync(Guid eventId, CancellationToken cancellationToken)
    {
        return await _dbContext.Events.Include(e => e.Guests)
                               .ThenInclude(g => g.Information)
                               .ThenInclude(i => i.Socials)
                               .SingleOrDefaultAsync(e => e.Id == eventId, cancellationToken);
    }

    /// <summary>
    ///     Asynchronously retrieves a paged list of guest records for the specified event.
    /// </summary>
    /// <param name="eventId">The unique identifier of the event for which to retrieve guest records.</param>
    /// <param name="page">The one-based page number to retrieve. Must be greater than or equal to 1.</param>
    /// <param name="pageSize">The maximum number of guest records to include in the page. Must be greater than 0.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a snapshot of guest records for the
    ///     specified page, including the total number of guests and the list of guest records for the page. If the page is
    ///     beyond the available data, the list will be empty.
    /// </returns>
    public async Task<GuestSnapshot> GetPageAsync(Guid eventId, int page, int pageSize, CancellationToken cancellationToken)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(page, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);

        var query = _dbContext.Guests.AsNoTracking()
                              .Where(g => g.Id == eventId);

        var total = await query.CountAsync(cancellationToken);
        var skip = (page - 1) * pageSize;

        var items = await query.OrderBy(g => g.Information.FirstName)
                               .ThenBy(g => g.Information.LastName)
                               .Skip(skip)
                               .Take(pageSize)
                               .Select(g => new GuestRecord(g.Information.FirstName + " " + g.Information.LastName, g.Information.Bio, g.Information.Socials.Imdb))
                               .ToListAsync(cancellationToken);

        return new GuestSnapshot(total, items);
    }

    /// <summary>
    ///     Asynchronously saves all changes made in the context to the underlying database.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the save operation.</param>
    /// <returns>A task that represents the asynchronous save operation.</returns>
    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}