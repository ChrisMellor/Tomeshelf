using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Tomeshelf.HumbleBundle.Application.Features.Bundles.Models;
using Tomeshelf.HumbleBundle.Domain.HumbleBundle;
using Tomeshelf.HumbleBundle.Infrastructure;
using Tomeshelf.HumbleBundle.Infrastructure.Bundles;

namespace Tomeshelf.HumbleBundle.Infrastructure.Tests.Bundles;

public class BundleIngestServiceTests
{
    [Fact]
    public async Task UpsertAsync_ReturnsZeros_WhenNoBundlesSupplied()
    {
        await using var context = CreateContext();
        context.Bundles.Add(new Bundle
        {
            MachineName = "existing-bundle",
            Title = "Existing"
        });
        await context.SaveChangesAsync();

        var service = new BundleIngestService(context, NullLogger<BundleIngestService>.Instance);
        var before = DateTimeOffset.UtcNow;

        var result = await service.UpsertAsync(Array.Empty<ScrapedBundle>(), CancellationToken.None);

        var after = DateTimeOffset.UtcNow;

        result.Created.Should().Be(0);
        result.Updated.Should().Be(0);
        result.Unchanged.Should().Be(0);
        result.Processed.Should().Be(0);
        result.ObservedAtUtc.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
        (await context.Bundles.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task UpsertAsync_CreatesNewBundles_AndPopulatesTimestamps()
    {
        await using var context = CreateContext();
        var service = new BundleIngestService(context, NullLogger<BundleIngestService>.Instance);

        var observedFirst = DateTimeOffset.UtcNow.AddMinutes(-10);
        var observedSecond = DateTimeOffset.UtcNow;
        var first = CreateScrapedBundle("bundle-one", observedFirst);
        var second = CreateScrapedBundle("bundle-two", observedSecond, title: "Bundle Two", url: "https://example.com/bundle-two");

        var result = await service.UpsertAsync(new List<ScrapedBundle> { first, second }, CancellationToken.None);

        result.Created.Should().Be(2);
        result.Updated.Should().Be(0);
        result.Unchanged.Should().Be(0);
        result.Processed.Should().Be(2);
        result.ObservedAtUtc.Should().Be(observedSecond);

        var entities = await context.Bundles.OrderBy(b => b.MachineName).ToListAsync();
        entities.Should().HaveCount(2);

        var createdFirst = entities[0];
        createdFirst.MachineName.Should().Be("bundle-one");
        createdFirst.FirstSeenUtc.Should().Be(observedFirst);
        createdFirst.LastSeenUtc.Should().Be(observedFirst);
        createdFirst.LastUpdatedUtc.Should().Be(observedFirst);
        createdFirst.Title.Should().Be("Bundle One");

        var createdSecond = entities[1];
        createdSecond.MachineName.Should().Be("bundle-two");
        createdSecond.FirstSeenUtc.Should().Be(observedSecond);
        createdSecond.LastSeenUtc.Should().Be(observedSecond);
        createdSecond.LastUpdatedUtc.Should().Be(observedSecond);
        createdSecond.Url.Should().Be("https://example.com/bundle-two");
    }

    [Fact]
    public async Task UpsertAsync_UpdatesExistingBundle_WhenFieldsChange()
    {
        await using var context = CreateContext();
        var existingFirstSeen = DateTimeOffset.UtcNow.AddDays(-3);
        var existingUpdated = DateTimeOffset.UtcNow.AddDays(-2);
        var existing = new Bundle
        {
            MachineName = "bundle-one",
            Category = "books",
            Stamp = "bundle",
            Title = "Old Title",
            ShortName = "Old",
            Url = "https://example.com/old",
            TileImageUrl = "tile-old",
            TileLogoUrl = "logo-old",
            HeroImageUrl = "hero-old",
            ShortDescription = "old desc",
            StartsAt = existingFirstSeen,
            EndsAt = existingFirstSeen.AddDays(1),
            FirstSeenUtc = existingFirstSeen,
            LastSeenUtc = existingUpdated,
            LastUpdatedUtc = existingUpdated
        };
        context.Bundles.Add(existing);
        await context.SaveChangesAsync();

        var service = new BundleIngestService(context, NullLogger<BundleIngestService>.Instance);
        var observed = DateTimeOffset.UtcNow;
        var scraped = CreateScrapedBundle("bundle-one", observed, title: "New Title", shortDescription: "new desc");

        var result = await service.UpsertAsync(new List<ScrapedBundle> { scraped }, CancellationToken.None);

        result.Created.Should().Be(0);
        result.Updated.Should().Be(1);
        result.Unchanged.Should().Be(0);
        result.Processed.Should().Be(1);

        var updated = await context.Bundles.SingleAsync();
        updated.FirstSeenUtc.Should().Be(existingFirstSeen);
        updated.LastSeenUtc.Should().Be(observed);
        updated.LastUpdatedUtc.Should().Be(observed);
        updated.Title.Should().Be("New Title");
        updated.ShortDescription.Should().Be("new desc");
    }

    [Fact]
    public async Task UpsertAsync_LeavesBundleUnchanged_WhenNoFieldsChange()
    {
        await using var context = CreateContext();
        var existingFirstSeen = DateTimeOffset.UtcNow.AddDays(-5);
        var existingUpdated = DateTimeOffset.UtcNow.AddDays(-3);
        var existing = new Bundle
        {
            MachineName = "bundle-one",
            Category = "books",
            Stamp = "bundle",
            Title = "Bundle One",
            ShortName = "Bundle",
            Url = "https://example.com/bundle-one",
            TileImageUrl = "tile",
            TileLogoUrl = "logo",
            HeroImageUrl = "hero",
            ShortDescription = "desc",
            StartsAt = existingFirstSeen,
            EndsAt = existingFirstSeen.AddDays(1),
            FirstSeenUtc = existingFirstSeen,
            LastSeenUtc = existingUpdated,
            LastUpdatedUtc = existingUpdated
        };
        context.Bundles.Add(existing);
        await context.SaveChangesAsync();

        var service = new BundleIngestService(context, NullLogger<BundleIngestService>.Instance);
        var observed = DateTimeOffset.UtcNow;
        var scraped = CreateScrapedBundle("bundle-one", observed, title: "Bundle One", shortName: "Bundle", url: "https://example.com/bundle-one", shortDescription: "desc", tileImageUrl: "tile", tileLogoUrl: "logo", heroImageUrl: "hero", startsAt: existingFirstSeen, endsAt: existingFirstSeen.AddDays(1));

        var result = await service.UpsertAsync(new List<ScrapedBundle> { scraped }, CancellationToken.None);

        result.Created.Should().Be(0);
        result.Updated.Should().Be(0);
        result.Unchanged.Should().Be(1);
        result.Processed.Should().Be(1);

        var updated = await context.Bundles.SingleAsync();
        updated.LastSeenUtc.Should().Be(observed);
        updated.LastUpdatedUtc.Should().Be(existingUpdated);
    }

    [Fact]
    public async Task UpsertAsync_ReusesEntity_WhenDuplicateMachineNamesScraped()
    {
        await using var context = CreateContext();
        var service = new BundleIngestService(context, NullLogger<BundleIngestService>.Instance);

        var firstObserved = DateTimeOffset.UtcNow.AddMinutes(-5);
        var secondObserved = DateTimeOffset.UtcNow;
        var first = CreateScrapedBundle("bundle-one", firstObserved, title: "First Title");
        var second = CreateScrapedBundle("bundle-one", secondObserved, title: "Second Title");

        var result = await service.UpsertAsync(new List<ScrapedBundle> { first, second }, CancellationToken.None);

        result.Created.Should().Be(1);
        result.Updated.Should().Be(1);
        result.Unchanged.Should().Be(0);
        result.Processed.Should().Be(2);

        var entity = await context.Bundles.SingleAsync();
        entity.Title.Should().Be("Second Title");
        entity.LastSeenUtc.Should().Be(secondObserved);
        entity.LastUpdatedUtc.Should().Be(secondObserved);
    }

    private static TomeshelfBundlesDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TomeshelfBundlesDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new TomeshelfBundlesDbContext(options);
    }

    private static ScrapedBundle CreateScrapedBundle(
        string machineName,
        DateTimeOffset observedUtc,
        string category = "books",
        string stamp = "bundle",
        string title = "Bundle One",
        string shortName = "Bundle",
        string url = "https://example.com/bundle-one",
        string tileImageUrl = "tile",
        string tileLogoUrl = "logo",
        string heroImageUrl = "hero",
        string shortDescription = "desc",
        DateTimeOffset? startsAt = null,
        DateTimeOffset? endsAt = null)
    {
        return new ScrapedBundle(machineName, category, stamp, title, shortName, url, tileImageUrl, tileLogoUrl, heroImageUrl, shortDescription, startsAt, endsAt, observedUtc);
    }
}
