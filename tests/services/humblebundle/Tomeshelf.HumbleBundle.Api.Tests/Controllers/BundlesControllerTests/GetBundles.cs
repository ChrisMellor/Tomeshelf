using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using Shouldly;
using Tomeshelf.Application.Shared.Abstractions.Messaging;
using Tomeshelf.HumbleBundle.Api.Controllers;
using Tomeshelf.HumbleBundle.Api.Tests.TestUtilities;
using Tomeshelf.HumbleBundle.Application.Features.Bundles.Commands;
using Tomeshelf.HumbleBundle.Application.Features.Bundles.Models;
using Tomeshelf.HumbleBundle.Application.Features.Bundles.Queries;
using Tomeshelf.HumbleBundle.Application.HumbleBundle;

namespace Tomeshelf.HumbleBundle.Api.Tests.Controllers.BundlesControllerTests;

public class GetBundles
{
    [Fact]
    public async Task ReturnsOk_WithMappedResponses()
    {
        var queryHandler = A.Fake<IQueryHandler<GetBundlesQuery, IReadOnlyList<BundleDto>>>();
        var refreshHandler = A.Fake<ICommandHandler<RefreshBundlesCommand, BundleIngestResult>>();
        var controller = BundlesControllerTestHarness.CreateController(queryHandler, refreshHandler);
        var now = DateTimeOffset.UtcNow;
        var dto = new BundleDto("bundle-one", "books", "bundle", "Bundle One", "One", "https://example.com/bundle-one", "tile", "logo", "hero", "desc", now.AddDays(-1), now.AddMinutes(5), now.AddDays(-3), now.AddDays(-1), now.AddMinutes(-10), now.AddMinutes(-1));

        A.CallTo(() => queryHandler.Handle(A<GetBundlesQuery>._, A<CancellationToken>._))
         .Returns(Task.FromResult<IReadOnlyList<BundleDto>>(new List<BundleDto> { dto }));

        var result = await controller.GetBundles(true, CancellationToken.None);

        var ok = result.Result.ShouldBeOfType<OkObjectResult>();
        var responses = ok.Value.ShouldBeAssignableTo<IReadOnlyList<BundlesController.BundleResponse>>();
        var response = responses.ShouldHaveSingleItem();

        response.MachineName.ShouldBe(dto.MachineName);
        response.Category.ShouldBe(dto.Category);
        response.Stamp.ShouldBe(dto.Stamp);
        response.Title.ShouldBe(dto.Title);
        response.ShortName.ShouldBe(dto.ShortName);
        response.Url.ShouldBe(dto.Url);
        response.TileImageUrl.ShouldBe(dto.TileImageUrl);
        response.TileLogoUrl.ShouldBe(dto.TileLogoUrl);
        response.HeroImageUrl.ShouldBe(dto.HeroImageUrl);
        response.ShortDescription.ShouldBe(dto.ShortDescription);
        response.StartsAt.ShouldBe(dto.StartsAt);
        response.EndsAt.ShouldBe(dto.EndsAt);
        response.FirstSeenUtc.ShouldBe(dto.FirstSeenUtc);
        response.LastSeenUtc.ShouldBe(dto.LastSeenUtc);
        response.LastUpdatedUtc.ShouldBe(dto.LastUpdatedUtc);
        response.GeneratedUtc.ShouldBe(dto.GeneratedUtc);
        response.SecondsRemaining.ShouldNotBeNull();
        (response.SecondsRemaining > 0).ShouldBeTrue();
        (response.SecondsRemaining < 301).ShouldBeTrue();

        A.CallTo(() => queryHandler.Handle(A<GetBundlesQuery>.That.Matches(query => query.IncludeExpired), A<CancellationToken>._))
         .MustHaveHappenedOnceExactly();
    }
}