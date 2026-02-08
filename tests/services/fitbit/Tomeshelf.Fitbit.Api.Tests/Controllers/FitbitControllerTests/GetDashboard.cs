using System.Net;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using Shouldly;
using Tomeshelf.Application.Shared.Abstractions.Messaging;
using Tomeshelf.Fitbit.Api.Tests.TestUtilities;
using Tomeshelf.Fitbit.Application;
using Tomeshelf.Fitbit.Application.Exceptions;
using Tomeshelf.Fitbit.Application.Features.Dashboard.Queries;
using Tomeshelf.Fitbit.Application.Features.Overview.Models;
using Tomeshelf.Fitbit.Application.Features.Overview.Queries;

namespace Tomeshelf.Fitbit.Api.Tests.Controllers.FitbitControllerTests;

public class GetDashboard
{
    [Fact]
    public async Task ReturnsBadGateway_WhenBadRequest()
    {
        // Arrange
        var handler = A.Fake<IQueryHandler<GetFitbitDashboardQuery, FitbitDashboardDto>>();
        var overviewHandler = A.Fake<IQueryHandler<GetFitbitOverviewQuery, FitbitOverviewDto>>();

        A.CallTo(() => handler.Handle(A<GetFitbitDashboardQuery>._, A<CancellationToken>._))
         .Throws(new FitbitBadRequestException("{\"message\":\"bad request\"}"));

        var controller = FitbitControllerTestHarness.CreateController(handler, overviewHandler);

        // Act
        var result = await controller.GetDashboard("2025-01-02", false, null, CancellationToken.None);

        // Assert
        var objectResult = result.Result.ShouldBeOfType<ObjectResult>();
        objectResult.StatusCode.ShouldBe(502);
    }

    [Fact]
    public async Task ReturnsNotFound_WhenSnapshotMissing()
    {
        // Arrange
        var handler = A.Fake<IQueryHandler<GetFitbitDashboardQuery, FitbitDashboardDto>>();
        var overviewHandler = A.Fake<IQueryHandler<GetFitbitOverviewQuery, FitbitOverviewDto>>();

        A.CallTo(() => handler.Handle(A<GetFitbitDashboardQuery>._, A<CancellationToken>._))
         .Returns(Task.FromResult<FitbitDashboardDto>(null));

        var controller = FitbitControllerTestHarness.CreateController(handler, overviewHandler);

        // Act
        var result = await controller.GetDashboard("2025-01-02", false, null, CancellationToken.None);

        // Assert
        result.Result.ShouldBeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task ReturnsOk_WhenSnapshotFound()
    {
        // Arrange
        var handler = A.Fake<IQueryHandler<GetFitbitDashboardQuery, FitbitDashboardDto>>();
        var overviewHandler = A.Fake<IQueryHandler<GetFitbitOverviewQuery, FitbitOverviewDto>>();
        var snapshot = FitbitControllerTestHarness.CreateSnapshot("2025-01-02");

        A.CallTo(() => handler.Handle(A<GetFitbitDashboardQuery>._, A<CancellationToken>._))
         .Returns(Task.FromResult(snapshot));

        var controller = FitbitControllerTestHarness.CreateController(handler, overviewHandler);

        // Act
        var result = await controller.GetDashboard("2025-01-02", false, "/fitness", CancellationToken.None);

        // Assert
        var ok = result.Result.ShouldBeOfType<OkObjectResult>();
        ok.Value.ShouldBeSameAs(snapshot);
    }

    [Fact]
    public async Task ReturnsRateLimit_WithRetryAfter()
    {
        // Arrange
        var handler = A.Fake<IQueryHandler<GetFitbitDashboardQuery, FitbitDashboardDto>>();
        var overviewHandler = A.Fake<IQueryHandler<GetFitbitOverviewQuery, FitbitOverviewDto>>();

        A.CallTo(() => handler.Handle(A<GetFitbitDashboardQuery>._, A<CancellationToken>._))
         .Throws(new FitbitRateLimitExceededException("{\"message\":\"rate limit\"}", TimeSpan.FromSeconds(9.2)));

        var controller = FitbitControllerTestHarness.CreateController(handler, overviewHandler);

        // Act
        var result = await controller.GetDashboard("2025-01-02", false, null, CancellationToken.None);

        // Assert
        var objectResult = result.Result.ShouldBeOfType<ObjectResult>();
        objectResult.StatusCode.ShouldBe(429);
        var retryAfterProperty = objectResult.Value!.GetType()
                                             .GetProperty("retryAfterSeconds");
        retryAfterProperty.ShouldNotBeNull();
        retryAfterProperty!.GetValue(objectResult.Value)
                           .ShouldBe(10);
    }

    [Fact]
    public async Task ReturnsRedirect_WhenUnauthorized()
    {
        // Arrange
        var handler = A.Fake<IQueryHandler<GetFitbitDashboardQuery, FitbitDashboardDto>>();
        var overviewHandler = A.Fake<IQueryHandler<GetFitbitOverviewQuery, FitbitOverviewDto>>();

        A.CallTo(() => handler.Handle(A<GetFitbitDashboardQuery>._, A<CancellationToken>._))
         .Throws(new HttpRequestException("unauthorized", null, HttpStatusCode.Unauthorized));

        var controller = FitbitControllerTestHarness.CreateController(handler, overviewHandler);

        // Act
        var result = await controller.GetDashboard("2025-01-02", false, "/dashboard", CancellationToken.None);

        // Assert
        var redirect = result.Result.ShouldBeOfType<RedirectResult>();
        redirect.Url.ShouldBe("https://example.test/authorize");
    }

    [Fact]
    public async Task ReturnsServiceUnavailable_WhenTimeout()
    {
        // Arrange
        var handler = A.Fake<IQueryHandler<GetFitbitDashboardQuery, FitbitDashboardDto>>();
        var overviewHandler = A.Fake<IQueryHandler<GetFitbitOverviewQuery, FitbitOverviewDto>>();

        A.CallTo(() => handler.Handle(A<GetFitbitDashboardQuery>._, A<CancellationToken>._))
         .Throws(new TaskCanceledException("timeout"));

        var controller = FitbitControllerTestHarness.CreateController(handler, overviewHandler);

        // Act
        var result = await controller.GetDashboard("2025-01-02", false, null, CancellationToken.None);

        // Assert
        var objectResult = result.Result.ShouldBeOfType<ObjectResult>();
        objectResult.StatusCode.ShouldBe(503);
    }
}