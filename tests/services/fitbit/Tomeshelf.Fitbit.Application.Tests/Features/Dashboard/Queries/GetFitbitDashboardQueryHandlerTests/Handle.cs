using Bogus;
using FakeItEasy;
using FluentAssertions;
using Tomeshelf.Fitbit.Application;
using Tomeshelf.Fitbit.Application.Abstractions.Services;
using Tomeshelf.Fitbit.Application.Features.Dashboard.Queries;

namespace Tomeshelf.Fitbit.Application.Tests.Features.Dashboard.Queries.GetFitbitDashboardQueryHandlerTests;

public class Handle
{
    [Fact]
    public async Task ValidQuery_CallsDashboardServiceAndReturnsResult()
    {
        // Arrange
        var faker = new Faker();
        var dashboardService = A.Fake<IFitbitDashboardService>();
        var handler = new GetFitbitDashboardQueryHandler(dashboardService);
        var date = DateOnly.FromDateTime(faker.Date.Recent());
        var dateString = date.ToString("yyyy-MM-dd");
        var expected = new FitbitDashboardDto
        {
            Date = dateString,
            Weight = new FitbitWeightSummaryDto(),
            Calories = new FitbitCaloriesSummaryDto(),
            Sleep = new FitbitSleepSummaryDto { Levels = new FitbitSleepLevelsDto() },
            Activity = new FitbitActivitySummaryDto(null, null, null)
        };

        A.CallTo(() => dashboardService.GetDashboardAsync(date, false, A<CancellationToken>._))
            .Returns(Task.FromResult(expected));

        var query = new GetFitbitDashboardQuery(date, false);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeSameAs(expected);
        A.CallTo(() => dashboardService.GetDashboardAsync(date, false, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }
}
