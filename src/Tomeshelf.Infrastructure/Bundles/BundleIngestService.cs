using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Domain.Entities.HumbleBundle;
using Tomeshelf.Infrastructure.Persistence;

namespace Tomeshelf.Infrastructure.Bundles;

/// <summary>
/// Persists scraped Humble Bundle listings by upserting bundle entities.
/// </summary>
public sealed class BundleIngestService
{
    private readonly TomeshelfBundlesDbContext _dbContext;
    private readonly ILogger<BundleIngestService> _logger;

    public BundleIngestService(TomeshelfBundlesDbContext dbContext, ILogger<BundleIngestService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// Upserts the provided bundle collection and returns ingest statistics.
    /// </summary>
    /// <param name="bundles">Bundles scraped from the Humble Bundle listing.</param>
    /// <param name="cancellationToken">Token used to cancel the database operation.</param>
    /// <returns>Summary of the ingest operation.</returns>
    public async Task<BundleIngestResult> UpsertAsync(IReadOnlyList<ScrapedBundle> bundles, CancellationToken cancellationToken = default)
    {
        if (bundles.Count == 0)
        {
            _logger.LogInformation("Bundle ingest skipped - no bundles were scraped.");
            return new BundleIngestResult(0, 0, 0, 0, DateTimeOffset.UtcNow);
        }

        var machineNames = bundles
            .Select(b => b.MachineName)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var existing = await _dbContext.Bundles
            .Where(b => machineNames.Contains(b.MachineName))
            .ToDictionaryAsync(b => b.MachineName, StringComparer.OrdinalIgnoreCase, cancellationToken);

        var counters = new IngestCounters();

        foreach (var scraped in bundles)
        {
            var isNew = false;

            if (!existing.TryGetValue(scraped.MachineName, out var entity))
            {
                entity = new Bundle
                {
                    MachineName = scraped.MachineName,
                    FirstSeenUtc = scraped.ObservedUtc
                };
                _dbContext.Bundles.Add(entity);
                existing[scraped.MachineName] = entity;
                counters.Created++;
                isNew = true;
            }

            var changed = UpdateEntity(entity, scraped);
            entity.LastSeenUtc = scraped.ObservedUtc;

            if (changed)
            {
                entity.LastUpdatedUtc = scraped.ObservedUtc;
                if (!isNew)
                {
                    counters.Updated++;
                }
            }
            else
            {
                counters.Unchanged++;
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        var observedAt = bundles.Max(b => b.ObservedUtc);
        _logger.LogInformation(
            "Bundle ingest complete - Created={Created}, Updated={Updated}, Unchanged={Unchanged}",
            counters.Created,
            counters.Updated,
            counters.Unchanged);

        return new BundleIngestResult(counters.Created, counters.Updated, counters.Unchanged, bundles.Count, observedAt);
    }

    private static bool UpdateEntity(Bundle entity, ScrapedBundle scraped)
    {
        var changed = false;

        changed |= SetIfDifferent(entity.Category, scraped.Category, value => entity.Category = value);
        changed |= SetIfDifferent(entity.Stamp, scraped.Stamp, value => entity.Stamp = value);
        changed |= SetIfDifferent(entity.Title, scraped.Title, value => entity.Title = value);
        changed |= SetIfDifferent(entity.ShortName, scraped.ShortName, value => entity.ShortName = value);
        changed |= SetIfDifferent(entity.Url, scraped.Url, value => entity.Url = value);
        changed |= SetIfDifferent(entity.TileImageUrl, scraped.TileImageUrl, value => entity.TileImageUrl = value);
        changed |= SetIfDifferent(entity.TileLogoUrl, scraped.TileLogoUrl, value => entity.TileLogoUrl = value);
        changed |= SetIfDifferent(entity.HeroImageUrl, scraped.HeroImageUrl, value => entity.HeroImageUrl = value);
        changed |= SetIfDifferent(entity.ShortDescription, scraped.ShortDescription, value => entity.ShortDescription = value);

        if (entity.StartsAt != scraped.StartsAt)
        {
            entity.StartsAt = scraped.StartsAt;
            changed = true;
        }

        if (entity.EndsAt != scraped.EndsAt)
        {
            entity.EndsAt = scraped.EndsAt;
            changed = true;
        }

        return changed;
    }

    private static bool SetIfDifferent(string current, string updated, Action<string> setter)
    {
        if (!string.Equals(current, updated, StringComparison.Ordinal))
        {
            setter(updated);
            return true;
        }

        return false;
    }

    private sealed class IngestCounters
    {
        public int Created { get; set; }
        public int Updated { get; set; }
        public int Unchanged { get; set; }
    }
}
