using FakeItEasy;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Tomeshelf.Fitbit.Infrastructure.Models;

namespace Tomeshelf.Fitbit.Infrastructure.Tests.FitbitDashboardServiceTests;

public sealed class GetDashboardAsync
{
    [Fact]
    public async Task ForceRefreshBypassesCacheAndRefetches()
    {
        // Arrange
        var client = A.Fake<IFitbitApiClient>();
        var cache = new MemoryCache(new MemoryCacheOptions());
        var options = new DbContextOptionsBuilder<TomeshelfFitbitDbContext>().UseInMemoryDatabase(Guid.NewGuid()
                                                                                                      .ToString("N"))
                                                                             .Options;

        await using var dbContext = new TomeshelfFitbitDbContext(options);
        await dbContext.Database.EnsureCreatedAsync();

        var date = DateOnly.FromDateTime(new DateTime(2025, 10, 15));
        var fetchCount = 0;

        A.CallTo(() => client.GetActivitiesAsync(date, A<CancellationToken>._))
         .ReturnsLazily(_ =>
          {
              fetchCount++;

              return Task.FromResult(new ActivitiesResponse
              {
                  Summary = new ActivitiesResponse.ActivitiesSummary
                  {
                      Steps = fetchCount,
                      Floors = 1,
                      CaloriesOut = 2000,
                      Distances = new List<ActivitiesResponse.ActivityDistance>()
                  }
              });
          });

        A.CallTo(() => client.GetCaloriesInAsync(date, A<CancellationToken>._))
         .Returns(Task.FromResult(new FoodLogSummaryResponse()));

        A.CallTo(() => client.GetWeightAsync(date, A<int>._, A<CancellationToken>._))
         .Returns(Task.FromResult(new WeightResponse()));

        A.CallTo(() => client.GetSleepAsync(date, A<CancellationToken>._))
         .Returns(Task.FromResult(new SleepResponse()));

        var service = new FitbitDashboardService(client, cache, dbContext, NullLogger<FitbitDashboardService>.Instance);

        // Act
        var first = await service.GetDashboardAsync(date, true, CancellationToken.None);
        var second = await service.GetDashboardAsync(date, false, CancellationToken.None);
        var third = await service.GetDashboardAsync(date, true, CancellationToken.None);

        // Assert
        first.Should()
             .NotBeNull();
        first!.Activity
              .Steps
              .Should()
              .Be(1);
        second.Should()
              .NotBeNull();
        second!.Activity
               .Steps
               .Should()
               .Be(1);
        third.Should()
             .NotBeNull();
        third!.Activity
              .Steps
              .Should()
              .Be(2);
    }

    [Fact]
    public async Task PopulatesBedtimeAndWakeTime_WhenSleepTimesContainFullTimestamps()
    {
        // Arrange
        var client = A.Fake<IFitbitApiClient>();
        var cache = new MemoryCache(new MemoryCacheOptions());
        var options = new DbContextOptionsBuilder<TomeshelfFitbitDbContext>().UseInMemoryDatabase(Guid.NewGuid()
                                                                                                      .ToString("N"))
                                                                             .Options;

        await using var dbContext = new TomeshelfFitbitDbContext(options);
        await dbContext.Database.EnsureCreatedAsync();

        var date = DateOnly.FromDateTime(new DateTime(2025, 10, 16));

        A.CallTo(() => client.GetActivitiesAsync(date, A<CancellationToken>._))
         .Returns(Task.FromResult(new ActivitiesResponse
          {
              Summary = new ActivitiesResponse.ActivitiesSummary
              {
                  CaloriesOut = 2000,
                  Steps = 5000,
                  Floors = 10,
                  Distances = new List<ActivitiesResponse.ActivityDistance>
                  {
                      new ActivitiesResponse.ActivityDistance
                      {
                          Activity = "total",
                          Distance = 3.5
                      }
                  }
              }
          }));

        A.CallTo(() => client.GetCaloriesInAsync(date, A<CancellationToken>._))
         .Returns(Task.FromResult(new FoodLogSummaryResponse { Summary = new FoodLogSummaryResponse.FoodSummary { Calories = 2100 } }));

        A.CallTo(() => client.GetWeightAsync(date, A<int>._, A<CancellationToken>._))
         .Returns(Task.FromResult(new WeightResponse()));

        A.CallTo(() => client.GetSleepAsync(date, A<CancellationToken>._))
         .Returns(Task.FromResult(new SleepResponse
          {
              Entries = new List<SleepResponse.SleepEntry>
              {
                  new SleepResponse.SleepEntry
                  {
                      DateOfSleep = "2025-10-16",
                      StartTime = "2025-10-16T22:15:00.000",
                      EndTime = "2025-10-17T06:45:00.000",
                      MinutesAsleep = 450,
                      MinutesAwake = 30,
                      Efficiency = 95,
                      Levels = new SleepResponse.SleepLevels
                      {
                          Summary = new SleepResponse.SleepLevelSummary
                          {
                              Deep = new SleepResponse.SleepLevelSummaryItem { Minutes = 90 },
                              Light = new SleepResponse.SleepLevelSummaryItem { Minutes = 270 },
                              Rem = new SleepResponse.SleepLevelSummaryItem { Minutes = 90 },
                              Wake = new SleepResponse.SleepLevelSummaryItem { Minutes = 30 }
                          }
                      }
                  }
              }
          }));

        var service = new FitbitDashboardService(client, cache, dbContext, NullLogger<FitbitDashboardService>.Instance);

        // Act
        var dashboard = await service.GetDashboardAsync(date, true, CancellationToken.None);

        // Assert
        dashboard.Should()
                 .NotBeNull();
        dashboard!.Sleep
                  .Bedtime
                  .Should()
                  .Be("22:15");
        dashboard.Sleep
                 .WakeTime
                 .Should()
                 .Be("06:45");
    }
}