using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Fitbit.Application;
using Tomeshelf.Fitbit.Application.Abstractions.Services;
using Tomeshelf.Fitbit.Application.Features.Dashboard.Queries;
using Xunit;

namespace Tomeshelf.Fitbit.Application.Tests.Features.Dashboard.Queries;

public class GetFitbitDashboardQueryHandlerTests
{
    [Fact]
    public async Task Handle_ValidQuery_CallsDashboardServiceAndReturnsResult()
    {
        // Arrange
        var dashboardService = new Mock<IFitbitDashboardService>();
        var handler = new GetFitbitDashboardQueryHandler(dashboardService.Object);
        var date = DateOnly.FromDateTime(new DateTime(2025, 10, 2));
        var expected = new FitbitDashboardDto
        {
            Date = "2025-10-02",
            Weight = new FitbitWeightSummaryDto(),
            Calories = new FitbitCaloriesSummaryDto(),
            Sleep = new FitbitSleepSummaryDto { Levels = new FitbitSleepLevelsDto() },
            Activity = new FitbitActivitySummaryDto(null, null, null)
        };

        dashboardService
            .Setup(s => s.GetDashboardAsync(date, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var query = new GetFitbitDashboardQuery(date, false);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Same(expected, result);
        dashboardService.Verify(s => s.GetDashboardAsync(date, false, It.IsAny<CancellationToken>()), Times.Once);
    }
}
