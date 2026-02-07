using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Tomeshelf.HumbleBundle.Application.Features.Bundles.Models;
using Tomeshelf.HumbleBundle.Domain.HumbleBundle;
using Tomeshelf.HumbleBundle.Infrastructure.Bundles;

namespace Tomeshelf.HumbleBundle.Infrastructure.Tests.Bundles.BundleIngestServiceTests;

public class UpsertAsync
{
    [Fact]
    public async Task CreatesNewBundles_AndPopulatesTimestamps()
    {
        // Arrange
        await using var context = CreateContext();
        var service = new BundleIngestService(context, NullLogger<BundleIngestService>.Instance);

        var observedFirst = DateTimeOffset.UtcNow.AddMinutes(-10);
        var observedSecond = DateTimeOffset.UtcNow;
        var first = CreateScrapedBundle("bundle-one", observedFirst);
        var second = CreateScrapedBundle("bundle-two", observedSecond, title: "Bundle Two", url: "https://example.com/bundle-two");

        // Act
        var result = await service.UpsertAsync(new List<ScrapedBundle>
        {
            first,
            second
        }, CancellationToken.None);

        // Assert
        result.Created.ShouldBe(2);
        result.Updated.ShouldBe(0);
        result.Unchanged.ShouldBe(0);
        result.Processed.ShouldBe(2);
        result.ObservedAtUtc.ShouldBe(observedSecond);

        var entities = await context.Bundles
                                    .OrderBy(b => b.MachineName)
                                    .ToListAsync();
        entities.Count.ShouldBe(2);

        var createdFirst = entities[0];
        createdFirst.MachineName.ShouldBe("bundle-one");
        createdFirst.FirstSeenUtc.ShouldBe(observedFirst);
        createdFirst.LastSeenUtc.ShouldBe(observedFirst);
        createdFirst.LastUpdatedUtc.ShouldBe(observedFirst);
        createdFirst.Title.ShouldBe("Bundle One");

        var createdSecond = entities[1];
        createdSecond.MachineName.ShouldBe("bundle-two");
        createdSecond.FirstSeenUtc.ShouldBe(observedSecond);
        createdSecond.LastSeenUtc.ShouldBe(observedSecond);
        createdSecond.LastUpdatedUtc.ShouldBe(observedSecond);
        createdSecond.Url.ShouldBe("https://example.com/bundle-two");
    }

    [Fact]
    public async Task LeavesBundleUnchanged_WhenNoFieldsChange()
    {
        // Arrange
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

        // Act
        var result = await service.UpsertAsync(new List<ScrapedBundle> { scraped }, CancellationToken.None);

        // Assert
        result.Created.ShouldBe(0);
        result.Updated.ShouldBe(0);
        result.Unchanged.ShouldBe(1);
        result.Processed.ShouldBe(1);

        var updated = await context.Bundles.SingleAsync();
        updated.LastSeenUtc.ShouldBe(observed);
        updated.LastUpdatedUtc.ShouldBe(existingUpdated);
    }

    [Fact]
    public async Task ReturnsZeros_WhenNoBundlesSupplied()
    {
        // Arrange
        await using var context = CreateContext();
        context.Bundles.Add(new Bundle
        {
            MachineName = "existing-bundle",
            Title = "Existing"
        });
        await context.SaveChangesAsync();

        var service = new BundleIngestService(context, NullLogger<BundleIngestService>.Instance);
        var before = DateTimeOffset.UtcNow;

        // Act
        var result = await service.UpsertAsync(Array.Empty<ScrapedBundle>(), CancellationToken.None);

        // Assert
        var after = DateTimeOffset.UtcNow;
        result.Created.ShouldBe(0);
        result.Updated.ShouldBe(0);
        result.Unchanged.ShouldBe(0);
        result.Processed.ShouldBe(0);
        result.ObservedAtUtc.ShouldBeInRange(before, after);
        (await context.Bundles.CountAsync()).ShouldBe(1);
    }

    [Fact]
    public async Task ReusesEntity_WhenDuplicateMachineNamesScraped()
    {
        // Arrange
        await using var context = CreateContext();
        var service = new BundleIngestService(context, NullLogger<BundleIngestService>.Instance);

        var firstObserved = DateTimeOffset.UtcNow.AddMinutes(-5);
        var secondObserved = DateTimeOffset.UtcNow;
        var first = CreateScrapedBundle("bundle-one", firstObserved, title: "First Title");
        var second = CreateScrapedBundle("bundle-one", secondObserved, title: "Second Title");

        // Act
        var result = await service.UpsertAsync(new List<ScrapedBundle>
        {
            first,
            second
        }, CancellationToken.None);

        // Assert
        result.Created.ShouldBe(1);
        result.Updated.ShouldBe(1);
        result.Unchanged.ShouldBe(0);
        result.Processed.ShouldBe(2);

        var entity = await context.Bundles.SingleAsync();
        entity.Title.ShouldBe("Second Title");
        entity.LastSeenUtc.ShouldBe(secondObserved);
        entity.LastUpdatedUtc.ShouldBe(secondObserved);
    }

    [Fact]
    public async Task UpdatesExistingBundle_WhenFieldsChange()
    {
        // Arrange
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

        // Act
        var result = await service.UpsertAsync(new List<ScrapedBundle> { scraped }, CancellationToken.None);

        // Assert
        result.Created.ShouldBe(0);
        result.Updated.ShouldBe(1);
        result.Unchanged.ShouldBe(0);
        result.Processed.ShouldBe(1);

        var updated = await context.Bundles.SingleAsync();
        updated.FirstSeenUtc.ShouldBe(existingFirstSeen);
        updated.LastSeenUtc.ShouldBe(observed);
        updated.LastUpdatedUtc.ShouldBe(observed);
        updated.Title.ShouldBe("New Title");
        updated.ShortDescription.ShouldBe("new desc");
    }

    private static TomeshelfBundlesDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TomeshelfBundlesDbContext>()
                      .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
                      .Options;

        return new TomeshelfBundlesDbContext(options);
    }

    private static ScrapedBundle CreateScrapedBundle(string machineName, DateTimeOffset observedUtc, string category = "books", string stamp = "bundle", string title = "Bundle One", string shortName = "Bundle", string url = "https://example.com/bundle-one", string tileImageUrl = "tile", string tileLogoUrl = "logo", string heroImageUrl = "hero", string shortDescription = "desc", DateTimeOffset? startsAt = null, DateTimeOffset? endsAt = null)
    {
        return new ScrapedBundle(machineName, category, stamp, title, shortName, url, tileImageUrl, tileLogoUrl, heroImageUrl, shortDescription, startsAt, endsAt, observedUtc);
    }
}
