using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Tomeshelf.Domain.Entities.HumbleBundle;
using Tomeshelf.HumbleBundle.Api.Controllers;
using Tomeshelf.Infrastructure.Bundles;
using Tomeshelf.Infrastructure.Persistence;

namespace Tomeshelf.HumbleBundle.Api.Tests.Controllers;

public sealed class BundlesControllerTests : IDisposable
{
    private readonly TomeshelfBundlesDbContext _dbContext;

    public BundlesControllerTests()
    {
        var options = new DbContextOptionsBuilder<TomeshelfBundlesDbContext>().UseInMemoryDatabase(Guid.NewGuid()
                                                                                                       .ToString())
                                                                              .Options;

        _dbContext = new TomeshelfBundlesDbContext(options);
    }

    public void Dispose()
    {
        _dbContext?.Dispose();
    }

    [Fact]
    public async Task GetBundles_ReturnsOnlyActive_WhenIncludeExpiredFalse()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        _dbContext.Bundles.AddRange(new Bundle
        {
                MachineName = "active_bundle",
                Category = "software",
                Stamp = "bundle",
                Title = "Active Bundle",
                ShortDescription = "Active bundle description",
                Url = "https://humblebundle.com/active",
                TileImageUrl = "https://img/active.png",
                StartsAt = now.AddDays(-1),
                EndsAt = now.AddDays(3),
                FirstSeenUtc = now.AddDays(-2),
                LastSeenUtc = now,
                LastUpdatedUtc = now
        }, new Bundle
        {
                MachineName = "expired_bundle",
                Category = "books",
                Stamp = "bundle",
                Title = "Expired Bundle",
                ShortDescription = "Expired bundle description",
                Url = "https://humblebundle.com/expired",
                TileImageUrl = "https://img/expired.png",
                StartsAt = now.AddDays(-10),
                EndsAt = now.AddDays(-2),
                FirstSeenUtc = now.AddDays(-11),
                LastSeenUtc = now.AddDays(-2),
                LastUpdatedUtc = now.AddDays(-2)
        });
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var queries = new BundleQueries(_dbContext);
        var ingest = new BundleIngestService(_dbContext, NullLogger<BundleIngestService>.Instance);
        var scraper = A.Fake<IHumbleBundleScraper>();
        var controller = new BundlesController(queries, scraper, ingest, NullLogger<BundlesController>.Instance);

        // Act
        var actionResult = await controller.GetBundles(false, TestContext.Current.CancellationToken);

        // Assert
        var okResult = actionResult.Result.Should()
                                   .BeOfType<OkObjectResult>()
                                   .Subject;
        var bundles = okResult.Value.Should()
                              .BeAssignableTo<IReadOnlyList<BundlesController.BundleResponse>>()
                              .Subject;

        bundles.Should()
               .ContainSingle(b => b.MachineName == "active_bundle");
        bundles.Should()
               .NotContain(b => b.MachineName == "expired_bundle");
    }

    [Fact]
    public async Task GetBundles_IncludesExpired_WhenRequested()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        _dbContext.Bundles.AddRange(new Bundle
        {
                MachineName = "active_bundle",
                Category = "software",
                Stamp = "bundle",
                Title = "Active Bundle",
                Url = "https://humblebundle.com/active",
                StartsAt = now.AddDays(-1),
                EndsAt = now.AddDays(5),
                FirstSeenUtc = now.AddDays(-2),
                LastSeenUtc = now,
                LastUpdatedUtc = now
        }, new Bundle
        {
                MachineName = "expired_bundle",
                Category = "books",
                Stamp = "bundle",
                Title = "Expired Bundle",
                Url = "https://humblebundle.com/expired",
                StartsAt = now.AddDays(-10),
                EndsAt = now.AddDays(-1),
                FirstSeenUtc = now.AddDays(-11),
                LastSeenUtc = now.AddDays(-1),
                LastUpdatedUtc = now.AddDays(-1)
        });
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var queries = new BundleQueries(_dbContext);
        var ingest = new BundleIngestService(_dbContext, NullLogger<BundleIngestService>.Instance);
        var scraper = A.Fake<IHumbleBundleScraper>();
        var controller = new BundlesController(queries, scraper, ingest, NullLogger<BundlesController>.Instance);

        // Act
        var actionResult = await controller.GetBundles(true, TestContext.Current.CancellationToken);

        // Assert
        var okResult = actionResult.Result.Should()
                                   .BeOfType<OkObjectResult>()
                                   .Subject;
        var bundles = okResult.Value.Should()
                              .BeAssignableTo<IReadOnlyList<BundlesController.BundleResponse>>()
                              .Subject;

        bundles.Should()
               .ContainSingle(b => b.MachineName == "active_bundle");
        bundles.Should()
               .ContainSingle(b => b.MachineName == "expired_bundle");
    }

    [Fact]
    public async Task RefreshBundles_UpsertsScrapedData_AndReturnsSummary()
    {
        // Arrange
        var scraper = A.Fake<IHumbleBundleScraper>();
        var observed = DateTimeOffset.UtcNow;
        var scrapedBundles = new List<ScrapedBundle>
        {
                new("refreshed_bundle", "games", "bundle", "Refreshed Bundle", "Refreshed", "https://humblebundle.com/refreshed", "https://img/refreshed.png", "https://img/refreshed-logo.png", "https://img/refreshed-hero.png", "Fresh content", observed.AddDays(-1), observed.AddDays(6), observed)
        };

        A.CallTo(() => scraper.ScrapeAsync(A<CancellationToken>.Ignored))
         .Returns(scrapedBundles);

        var queries = new BundleQueries(_dbContext);
        var ingest = new BundleIngestService(_dbContext, NullLogger<BundleIngestService>.Instance);
        var controller = new BundlesController(queries, scraper, ingest, NullLogger<BundlesController>.Instance);

        // Act
        var actionResult = await controller.RefreshBundles(TestContext.Current.CancellationToken);

        // Assert
        var okResult = actionResult.Result.Should()
                                   .BeOfType<OkObjectResult>()
                                   .Subject;
        var payload = okResult.Value.Should()
                              .BeAssignableTo<BundlesController.RefreshBundlesResponse>()
                              .Subject;

        payload.Created.Should()
               .Be(1);
        payload.Updated.Should()
               .Be(0);
        payload.Unchanged.Should()
               .Be(0);
        payload.Processed.Should()
               .Be(1);
        payload.ObservedAtUtc.Should()
               .Be(observed);

        var bundle = await _dbContext.Bundles.SingleAsync(b => b.MachineName == "refreshed_bundle", TestContext.Current.CancellationToken);
        bundle.Title.Should()
              .Be("Refreshed Bundle");
    }
}