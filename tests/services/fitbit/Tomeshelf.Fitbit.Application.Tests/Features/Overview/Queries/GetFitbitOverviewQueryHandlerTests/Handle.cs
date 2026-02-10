using Bogus;
using FakeItEasy;
using Shouldly;
using Tomeshelf.Fitbit.Application.Abstractions.Services;
using Tomeshelf.Fitbit.Application.Features.Overview.Models;
using Tomeshelf.Fitbit.Application.Features.Overview.Queries;

namespace Tomeshelf.Fitbit.Application.Tests.Features.Overview.Queries.GetFitbitOverviewQueryHandlerTests;

public class Handle
{
    /// <summary>
    ///     Calls overview service and returns result when the query is valid.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task ValidQuery_CallsOverviewServiceAndReturnsResult()
    {
        // Arrange
        var faker = new Faker();
        var overviewService = A.Fake<IFitbitOverviewService>();
        var handler = new GetFitbitOverviewQueryHandler(overviewService);
        var date = DateOnly.FromDateTime(faker.Date.Recent());
        var dateString = date.ToString("yyyy-MM-dd");
        var expected = new FitbitOverviewDto
        {
            Daily = new FitbitDashboardDto
            {
                Date = dateString,
                Weight = new FitbitWeightSummaryDto(),
                Calories = new FitbitCaloriesSummaryDto(),
                Sleep = new FitbitSleepSummaryDto { Levels = new FitbitSleepLevelsDto() },
                Activity = new FitbitActivitySummaryDto(null, null, null)
            },
            Last7Days = new FitbitOverviewRangeDto
            {
                Days = 7,
                Items = Array.Empty<FitbitOverviewDayDto>()
            },
            Last30Days = new FitbitOverviewRangeDto
            {
                Days = 30,
                Items = Array.Empty<FitbitOverviewDayDto>()
            }
        };

        A.CallTo(() => overviewService.GetOverviewAsync(date, true, A<CancellationToken>._))
         .Returns(Task.FromResult(expected));

        var query = new GetFitbitOverviewQuery(date, true);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldBeSameAs(expected);
        A.CallTo(() => overviewService.GetOverviewAsync(date, true, A<CancellationToken>._))
         .MustHaveHappenedOnceExactly();
    }
}