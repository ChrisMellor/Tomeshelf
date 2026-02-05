using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tomeshelf.Web.Controllers;
using Tomeshelf.Web.Models.Bundles;
using Tomeshelf.Web.Services;

namespace Tomeshelf.Web.Tests.Controllers.BundlesControllerTests;

public class Index
{
    private const string LastViewedCookieName = "tomeshelf_bundles_lastViewedUtc";

    [Fact]
    public async Task BuildsGroupedBundlesAndSetsCookie()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var lastViewed = now.AddDays(-2);

        var bundles = new List<BundleModel>
        {
            new BundleModel
            {
                MachineName = "bundle-one",
                Category = null,
                Stamp = "games",
                Title = null,
                ShortName = "Bundle One",
                Url = "https://example.test/one",
                TileImageUrl = "tile",
                TileLogoUrl = "logo",
                HeroImageUrl = "hero",
                ShortDescription = "desc",
                StartsAt = now.AddDays(-1),
                EndsAt = now.AddDays(2),
                FirstSeenUtc = now.AddDays(-1),
                LastSeenUtc = now.AddHours(-1),
                LastUpdatedUtc = now.AddDays(-1),
                SecondsRemaining = 123,
                GeneratedUtc = new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero)
            },
            new BundleModel
            {
                MachineName = "bundle-two",
                Category = "books",
                Stamp = null,
                Title = "Bundle Two",
                ShortName = "B2",
                Url = "https://example.test/two",
                TileImageUrl = "tile2",
                TileLogoUrl = "logo2",
                HeroImageUrl = "hero2",
                ShortDescription = "desc2",
                StartsAt = now.AddDays(-10),
                EndsAt = now.AddDays(5),
                FirstSeenUtc = now.AddDays(-10),
                LastSeenUtc = now.AddDays(-1),
                LastUpdatedUtc = now.AddHours(-12),
                SecondsRemaining = 456,
                GeneratedUtc = new DateTimeOffset(2020, 1, 2, 0, 0, 0, TimeSpan.Zero)
            },
            new BundleModel
            {
                MachineName = "bundle-expired",
                Category = string.Empty,
                Stamp = string.Empty,
                Title = "Expired",
                ShortName = "Expired",
                Url = "https://example.test/expired",
                TileImageUrl = "tile3",
                TileLogoUrl = "logo3",
                HeroImageUrl = "hero3",
                ShortDescription = "desc3",
                StartsAt = now.AddDays(-10),
                EndsAt = now.AddDays(-1),
                FirstSeenUtc = now.AddDays(-10),
                LastSeenUtc = now.AddDays(-1),
                LastUpdatedUtc = now.AddDays(-5),
                SecondsRemaining = null,
                GeneratedUtc = new DateTimeOffset(2020, 1, 3, 0, 0, 0, TimeSpan.Zero)
            }
        };

        var api = A.Fake<IBundlesApi>();
        A.CallTo(() => api.GetBundlesAsync(false, A<CancellationToken>._))
         .Returns(bundles);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Cookie = $"{LastViewedCookieName}={lastViewed:O}";

        var controller = new BundlesController(api, A.Fake<IFileUploadsApi>()) { ControllerContext = new ControllerContext { HttpContext = httpContext } };

        // Act
        var result = await controller.Index(false, CancellationToken.None);

        // Assert
        var view = result.Should()
                         .BeOfType<ViewResult>()
                         .Subject;
        var model = view.Model
                        .Should()
                        .BeOfType<BundlesIndexViewModel>()
                        .Subject;

        model.ActiveBundles
             .Should()
             .HaveCount(2);
        model.ExpiredBundles
             .Should()
             .HaveCount(1);
        model.NewBundlesCount
             .Should()
             .Be(1);
        model.UpdatedBundlesCount
             .Should()
             .Be(1);
        model.DataTimestampUtc
             .Should()
             .Be(new DateTimeOffset(2020, 1, 3, 0, 0, 0, TimeSpan.Zero));

        var categories = model.ActiveBundles
                              .Select(group => group.Category)
                              .ToList();
        categories.Should()
                  .Contain(new[] { "Books", "Games" });

        var gamesBundle = model.ActiveBundles
                               .SelectMany(group => group.Bundles)
                               .Single(bundle => bundle.MachineName == "bundle-one");
        gamesBundle.Title
                   .Should()
                   .Be("Bundle One");
        gamesBundle.TimeRemaining
                   .Should()
                   .NotBeNull();
        gamesBundle.IsNewSinceLastFetch
                   .Should()
                   .BeTrue();

        var expired = model.ExpiredBundles.Single();
        expired.IsExpired
               .Should()
               .BeTrue();
        expired.TimeRemaining
               .Should()
               .BeNull();

        httpContext.Response
                   .Headers["Set-Cookie"]
                   .ToString()
                   .Should()
                   .Contain(LastViewedCookieName);
    }
}