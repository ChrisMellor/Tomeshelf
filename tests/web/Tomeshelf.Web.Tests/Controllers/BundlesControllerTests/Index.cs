using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shouldly;
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
        var now = DateTimeOffset.UtcNow;
        var lastViewed = now.AddDays(-2);

        var bundles = new List<BundleModel>
        {
            new()
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
            new()
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
            new()
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

        var result = await controller.Index(false, CancellationToken.None);

        var view = result.ShouldBeOfType<ViewResult>();
        var model = view.Model.ShouldBeOfType<BundlesIndexViewModel>();

        model.ActiveBundles.Count.ShouldBe(2);
        model.ExpiredBundles.ShouldHaveSingleItem();
        model.NewBundlesCount.ShouldBe(1);
        model.UpdatedBundlesCount.ShouldBe(1);
        model.DataTimestampUtc.ShouldBe(new DateTimeOffset(2020, 1, 3, 0, 0, 0, TimeSpan.Zero));

        var categories = model.ActiveBundles
                              .Select(group => group.Category)
                              .ToList();
        categories.ShouldContain("Books");
        categories.ShouldContain("Games");

        var gamesBundle = model.ActiveBundles
                               .SelectMany(group => group.Bundles)
                               .Single(bundle => bundle.MachineName == "bundle-one");
        gamesBundle.Title.ShouldBe("Bundle One");
        gamesBundle.TimeRemaining.ShouldNotBeNull();
        gamesBundle.IsNewSinceLastFetch.ShouldBeTrue();

        var expired = model.ExpiredBundles.Single();
        expired.IsExpired.ShouldBeTrue();
        expired.TimeRemaining.ShouldBeNull();

        httpContext.Response
                   .Headers["Set-Cookie"]
                   .ToString()
                   .ShouldContain(LastViewedCookieName);
    }
}