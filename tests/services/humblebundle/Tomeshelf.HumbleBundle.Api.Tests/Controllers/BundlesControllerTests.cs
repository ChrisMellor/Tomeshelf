using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Tomeshelf.Application.Shared.Abstractions.Messaging;
using Tomeshelf.HumbleBundle.Api.Controllers;
using Tomeshelf.HumbleBundle.Application.Features.Bundles.Commands;
using Tomeshelf.HumbleBundle.Application.Features.Bundles.Models;
using Tomeshelf.HumbleBundle.Application.Features.Bundles.Queries;
using Tomeshelf.HumbleBundle.Application.HumbleBundle;

namespace Tomeshelf.HumbleBundle.Api.Tests.Controllers;

public class BundlesControllerTests
{
    [Fact]
    public void BundleResponse_FromDto_ReturnsNullSeconds_WhenAlreadyExpired()
    {
        var now = DateTimeOffset.UtcNow;
        var dto = CreateDto(now.AddMinutes(-5));

        var response = BundlesController.BundleResponse.FromDto(dto, now);

        response.SecondsRemaining
                .Should()
                .BeNull();
    }

    [Fact]
    public void BundleResponse_FromDto_ReturnsNullSeconds_WhenNoEndDate()
    {
        var dto = CreateDto(null);

        var response = BundlesController.BundleResponse.FromDto(dto, DateTimeOffset.UtcNow);

        response.SecondsRemaining
                .Should()
                .BeNull();
    }

    [Fact]
    public async Task GetBundles_ReturnsOk_WithMappedResponses()
    {
        var queryHandler = A.Fake<IQueryHandler<GetBundlesQuery, IReadOnlyList<BundleDto>>>();
        var refreshHandler = A.Fake<ICommandHandler<RefreshBundlesCommand, BundleIngestResult>>();
        var controller = CreateController(queryHandler, refreshHandler);
        var now = DateTimeOffset.UtcNow;
        var dto = new BundleDto("bundle-one", "books", "bundle", "Bundle One", "One", "https://example.com/bundle-one", "tile", "logo", "hero", "desc", now.AddDays(-1), now.AddMinutes(5), now.AddDays(-3), now.AddDays(-1), now.AddMinutes(-10), now.AddMinutes(-1));

        A.CallTo(() => queryHandler.Handle(A<GetBundlesQuery>._, A<CancellationToken>._))
         .Returns(Task.FromResult<IReadOnlyList<BundleDto>>(new List<BundleDto> { dto }));

        var result = await controller.GetBundles(true, CancellationToken.None);

        var ok = result.Result
                       .Should()
                       .BeOfType<OkObjectResult>()
                       .Subject;
        var response = ok.Value
                         .Should()
                         .BeAssignableTo<IReadOnlyList<BundlesController.BundleResponse>>()
                         .Subject
                         .Single();

        response.MachineName
                .Should()
                .Be(dto.MachineName);
        response.Category
                .Should()
                .Be(dto.Category);
        response.Stamp
                .Should()
                .Be(dto.Stamp);
        response.Title
                .Should()
                .Be(dto.Title);
        response.ShortName
                .Should()
                .Be(dto.ShortName);
        response.Url
                .Should()
                .Be(dto.Url);
        response.TileImageUrl
                .Should()
                .Be(dto.TileImageUrl);
        response.TileLogoUrl
                .Should()
                .Be(dto.TileLogoUrl);
        response.HeroImageUrl
                .Should()
                .Be(dto.HeroImageUrl);
        response.ShortDescription
                .Should()
                .Be(dto.ShortDescription);
        response.StartsAt
                .Should()
                .Be(dto.StartsAt);
        response.EndsAt
                .Should()
                .Be(dto.EndsAt);
        response.FirstSeenUtc
                .Should()
                .Be(dto.FirstSeenUtc);
        response.LastSeenUtc
                .Should()
                .Be(dto.LastSeenUtc);
        response.LastUpdatedUtc
                .Should()
                .Be(dto.LastUpdatedUtc);
        response.GeneratedUtc
                .Should()
                .Be(dto.GeneratedUtc);
        response.SecondsRemaining
                .Should()
                .BeGreaterThan(0)
                .And
                .BeLessThan(301);

        A.CallTo(() => queryHandler.Handle(A<GetBundlesQuery>.That.Matches(query => query.IncludeExpired), A<CancellationToken>._))
         .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task RefreshBundles_ReturnsOk_WithIngestSummary()
    {
        var queryHandler = A.Fake<IQueryHandler<GetBundlesQuery, IReadOnlyList<BundleDto>>>();
        var refreshHandler = A.Fake<ICommandHandler<RefreshBundlesCommand, BundleIngestResult>>();
        var controller = CreateController(queryHandler, refreshHandler);
        var observed = DateTimeOffset.UtcNow;
        var ingestResult = new BundleIngestResult(1, 2, 3, 6, observed);

        A.CallTo(() => refreshHandler.Handle(A<RefreshBundlesCommand>._, A<CancellationToken>._))
         .Returns(Task.FromResult(ingestResult));

        var result = await controller.RefreshBundles(CancellationToken.None);

        var ok = result.Result
                       .Should()
                       .BeOfType<OkObjectResult>()
                       .Subject;
        var response = ok.Value
                         .Should()
                         .BeOfType<BundlesController.RefreshBundlesResponse>()
                         .Subject;

        response.Created
                .Should()
                .Be(1);
        response.Updated
                .Should()
                .Be(2);
        response.Unchanged
                .Should()
                .Be(3);
        response.Processed
                .Should()
                .Be(6);
        response.ObservedAtUtc
                .Should()
                .Be(observed);

        A.CallTo(() => refreshHandler.Handle(A<RefreshBundlesCommand>._, A<CancellationToken>._))
         .MustHaveHappenedOnceExactly();
    }

    private static BundlesController CreateController(IQueryHandler<GetBundlesQuery, IReadOnlyList<BundleDto>> queryHandler, ICommandHandler<RefreshBundlesCommand, BundleIngestResult> refreshHandler)
    {
        return new BundlesController(queryHandler, refreshHandler, NullLogger<BundlesController>.Instance);
    }

    private static BundleDto CreateDto(DateTimeOffset? endsAt)
    {
        var now = DateTimeOffset.UtcNow;

        return new BundleDto("bundle-one", "books", "bundle", "Bundle One", "One", "https://example.com/bundle-one", "tile", "logo", "hero", "desc", now.AddDays(-1), endsAt, now.AddDays(-3), now.AddDays(-2), now.AddMinutes(-10), now.AddMinutes(-5));
    }
}