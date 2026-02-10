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

public class RefreshBundles
{
    /// <summary>
    ///     Returns ok with ingest summary.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task ReturnsOk_WithIngestSummary()
    {
        // Arrange
        var queryHandler = A.Fake<IQueryHandler<GetBundlesQuery, IReadOnlyList<BundleDto>>>();
        var refreshHandler = A.Fake<ICommandHandler<RefreshBundlesCommand, BundleIngestResult>>();
        var controller = BundlesControllerTestHarness.CreateController(queryHandler, refreshHandler);
        var observed = DateTimeOffset.UtcNow;
        var ingestResult = new BundleIngestResult(1, 2, 3, 6, observed);

        A.CallTo(() => refreshHandler.Handle(A<RefreshBundlesCommand>._, A<CancellationToken>._))
         .Returns(Task.FromResult(ingestResult));

        // Act
        var result = await controller.RefreshBundles(CancellationToken.None);

        // Assert
        var ok = result.Result.ShouldBeOfType<OkObjectResult>();
        var response = ok.Value.ShouldBeOfType<BundlesController.RefreshBundlesResponse>();

        response.Created.ShouldBe(1);
        response.Updated.ShouldBe(2);
        response.Unchanged.ShouldBe(3);
        response.Processed.ShouldBe(6);
        response.ObservedAtUtc.ShouldBe(observed);

        A.CallTo(() => refreshHandler.Handle(A<RefreshBundlesCommand>._, A<CancellationToken>._))
         .MustHaveHappenedOnceExactly();
    }
}