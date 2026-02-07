using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using Tomeshelf.Application.Shared.Abstractions.Messaging;
using Tomeshelf.Fitbit.Api.Tests.TestUtilities;
using Tomeshelf.Fitbit.Application;
using Tomeshelf.Fitbit.Application.Features.Dashboard.Queries;
using Tomeshelf.Fitbit.Application.Features.Overview.Models;
using Tomeshelf.Fitbit.Application.Features.Overview.Queries;

namespace Tomeshelf.Fitbit.Api.Tests.Controllers.FitbitControllerTests;

public class GetOverview
{
    [Fact]
    public async Task ReturnsNotFound_WhenOverviewMissing()
    {
        // Arrange
        var handler = A.Fake<IQueryHandler<GetFitbitDashboardQuery, FitbitDashboardDto>>();
        var overviewHandler = A.Fake<IQueryHandler<GetFitbitOverviewQuery, FitbitOverviewDto>>();

        A.CallTo(() => overviewHandler.Handle(A<GetFitbitOverviewQuery>._, A<CancellationToken>._))
         .Returns(Task.FromResult<FitbitOverviewDto>(null));

        var controller = FitbitControllerTestHarness.CreateController(handler, overviewHandler);

        // Act
        var result = await controller.GetOverview("2025-01-02", false, null, CancellationToken.None);

        // Assert
        result.Result.ShouldBeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task ReturnsOk_WhenOverviewFound()
    {
        // Arrange
        var handler = A.Fake<IQueryHandler<GetFitbitDashboardQuery, FitbitDashboardDto>>();
        var overviewHandler = A.Fake<IQueryHandler<GetFitbitOverviewQuery, FitbitOverviewDto>>();
        var overview = new FitbitOverviewDto
        {
            Daily = FitbitControllerTestHarness.CreateSnapshot("2025-01-02"),
            Last7Days = new FitbitOverviewRangeDto
            {
                Days = 7,
                Items = new List<FitbitOverviewDayDto>()
            },
            Last30Days = new FitbitOverviewRangeDto
            {
                Days = 30,
                Items = new List<FitbitOverviewDayDto>()
            }
        };

        A.CallTo(() => overviewHandler.Handle(A<GetFitbitOverviewQuery>._, A<CancellationToken>._))
         .Returns(Task.FromResult(overview));

        var controller = FitbitControllerTestHarness.CreateController(handler, overviewHandler);

        // Act
        var result = await controller.GetOverview("2025-01-02", false, null, CancellationToken.None);

        // Assert
        var ok = result.Result.ShouldBeOfType<OkObjectResult>();
        ok.Value.ShouldBeSameAs(overview);
    }
}