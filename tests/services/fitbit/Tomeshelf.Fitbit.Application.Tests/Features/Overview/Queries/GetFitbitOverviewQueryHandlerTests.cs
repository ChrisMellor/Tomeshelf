using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Fitbit.Application;
using Tomeshelf.Fitbit.Application.Abstractions.Services;
using Tomeshelf.Fitbit.Application.Features.Overview.Models;
using Tomeshelf.Fitbit.Application.Features.Overview.Queries;
using Xunit;

namespace Tomeshelf.Fitbit.Application.Tests.Features.Overview.Queries;

public class GetFitbitOverviewQueryHandlerTests
{
    [Fact]
    public async Task Handle_ValidQuery_CallsOverviewServiceAndReturnsResult()
    {
        // Arrange
        var overviewService = new Mock<IFitbitOverviewService>();
        var handler = new GetFitbitOverviewQueryHandler(overviewService.Object);
        var date = DateOnly.FromDateTime(new DateTime(2025, 10, 1));
        var expected = new FitbitOverviewDto
        {
            Daily = new FitbitDashboardDto
            {
                Date = "2025-10-01",
                Weight = new FitbitWeightSummaryDto(),
                Calories = new FitbitCaloriesSummaryDto(),
                Sleep = new FitbitSleepSummaryDto { Levels = new FitbitSleepLevelsDto() },
                Activity = new FitbitActivitySummaryDto(null, null, null)
            },
            Last7Days = new FitbitOverviewRangeDto { Days = 7, Items = Array.Empty<FitbitOverviewDayDto>() },
            Last30Days = new FitbitOverviewRangeDto { Days = 30, Items = Array.Empty<FitbitOverviewDayDto>() }
        };

        overviewService
            .Setup(s => s.GetOverviewAsync(date, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var query = new GetFitbitOverviewQuery(date, true);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Same(expected, result);
        overviewService.Verify(s => s.GetOverviewAsync(date, true, It.IsAny<CancellationToken>()), Times.Once);
    }
}
