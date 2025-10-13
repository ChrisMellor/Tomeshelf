using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Tomeshelf.Application.Contracts;
using Tomeshelf.Infrastructure.Persistence;

namespace Tomeshelf.Infrastructure.Bundles;

/// <summary>
///     Query helpers for retrieving Humble Bundle listings.
/// </summary>
public sealed class BundleQueries
{
    private readonly TomeshelfBundlesDbContext _dbContext;

    public BundleQueries(TomeshelfBundlesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    ///     Returns the most recent bundle listings, optionally filtering out expired bundles.
    /// </summary>
    /// <param name="includeExpired">When <c>true</c>, returns all bundles including expired ones.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of bundle DTOs.</returns>
    public async Task<IReadOnlyList<BundleDto>> GetBundlesAsync(bool includeExpired, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;

        var query = _dbContext.Bundles.AsNoTracking();
        if (!includeExpired)
        {
            query = query.Where(b => !b.EndsAt.HasValue || (b.EndsAt >= now));
        }

        var generatedAt = DateTimeOffset.UtcNow;

        return await query.OrderBy(b => b.EndsAt.HasValue
                                           ? 0
                                           : 1)
                          .ThenBy(b => b.EndsAt)
                          .ThenBy(b => b.Title)
                          .Select(b => new BundleDto(b.MachineName, b.Category, b.Stamp, b.Title, b.ShortName, b.Url, b.TileImageUrl, b.TileLogoUrl, b.HeroImageUrl, b.ShortDescription, b.StartsAt, b.EndsAt, b.FirstSeenUtc, b.LastSeenUtc, b.LastUpdatedUtc, generatedAt))
                          .ToListAsync(cancellationToken);
    }
}