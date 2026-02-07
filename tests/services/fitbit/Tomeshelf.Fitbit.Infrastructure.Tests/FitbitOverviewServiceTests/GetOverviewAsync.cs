using FakeItEasy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Tomeshelf.Fitbit.Application;
using Tomeshelf.Fitbit.Application.Abstractions.Services;
using Tomeshelf.Fitbit.Domain;

namespace Tomeshelf.Fitbit.Infrastructure.Tests.FitbitOverviewServiceTests;

public class GetOverviewAsync
{
    [Fact]
    public async Task ReturnsNull_WhenDashboardMissing()
    {
        // Arrange
        var dashboardService = A.Fake<IFitbitDashboardService>();
        A.CallTo(() => dashboardService.GetDashboardAsync(A<DateOnly>._, A<bool>._, A<CancellationToken>._))
         .Returns(Task.FromResult<FitbitDashboardDto>(null));

        await using var dbContext = CreateContext();
        var service = new FitbitOverviewService(dashboardService, dbContext, NullLogger<FitbitOverviewService>.Instance);

        // Act
        var result = await service.GetOverviewAsync(new DateOnly(2025, 1, 10), true, CancellationToken.None);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task UsesLastKnownWeightAcrossRange()
    {
        // Arrange
        var dashboardService = A.Fake<IFitbitDashboardService>();
        var dailySnapshot = CreateSnapshot("2025-01-10");
        A.CallTo(() => dashboardService.GetDashboardAsync(new DateOnly(2025, 1, 10), true, A<CancellationToken>._))
         .Returns(Task.FromResult(dailySnapshot));

        await using var dbContext = CreateContext();
        var generated = DateTimeOffset.UtcNow;

        dbContext.DailySnapshots.AddRange(
            new FitbitDailySnapshot
            {
                Date = new DateOnly(2025, 1, 3),
                StartingWeightKg = 80,
                GeneratedUtc = generated
            },
            new FitbitDailySnapshot
            {
                Date = new DateOnly(2025, 1, 6),
                CurrentWeightKg = 82,
                GeneratedUtc = generated
            },
            new FitbitDailySnapshot
            {
                Date = new DateOnly(2025, 1, 10),
                Steps = 5000,
                GeneratedUtc = generated
            });

        await dbContext.SaveChangesAsync();

        var service = new FitbitOverviewService(dashboardService, dbContext, NullLogger<FitbitOverviewService>.Instance);

        // Act
        var result = await service.GetOverviewAsync(new DateOnly(2025, 1, 10), true, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result!.Daily.ShouldBeSameAs(dailySnapshot);
        result.Last7Days.Items.Count.ShouldBe(7);

        var weightBeforeUpdate = result.Last7Days.Items.First(item => item.Date == "2025-01-05");
        weightBeforeUpdate.WeightKg.ShouldBe(80);

        var weightAfterUpdate = result.Last7Days.Items.First(item => item.Date == "2025-01-10");
        weightAfterUpdate.WeightKg.ShouldBe(82);
    }

    private static TomeshelfFitbitDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TomeshelfFitbitDbContext>()
                      .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
                      .Options;

        var context = new TomeshelfFitbitDbContext(options);
        context.Database.EnsureCreated();

        return context;
    }

    private static FitbitDashboardDto CreateSnapshot(string date)
    {
        return new FitbitDashboardDto
        {
            Date = date,
            Weight = new FitbitWeightSummaryDto
            {
                StartingWeightKg = 80,
                CurrentWeightKg = 80,
                ChangeKg = 0
            },
            Calories = new FitbitCaloriesSummaryDto
            {
                IntakeCalories = 2000,
                BurnedCalories = 1800,
                NetCalories = -200
            },
            Sleep = new FitbitSleepSummaryDto
            {
                TotalSleepHours = 7,
                TotalAwakeHours = 1,
                EfficiencyPercentage = 85,
                Bedtime = "22:00",
                WakeTime = "06:00",
                Levels = new FitbitSleepLevelsDto
                {
                    DeepMinutes = 60,
                    LightMinutes = 240,
                    RemMinutes = 90,
                    WakeMinutes = 30
                }
            },
            Activity = new FitbitActivitySummaryDto(5000, 4.2, 10)
        };
    }
}
