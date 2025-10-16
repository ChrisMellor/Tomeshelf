using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Tomeshelf.Infrastructure.Fitness;
using Tomeshelf.Infrastructure.Fitness.Models;
using Tomeshelf.Infrastructure.Persistence;
using Xunit;

namespace Tomeshelf.Infrastructure.Tests.Fitness;

public sealed class FitbitDashboardServiceTests
{
    [Fact]
    public async Task GetDashboardAsync_PopulatesBedtimeAndWakeTime_WhenSleepTimesContainFullTimestamps()
    {
        // Arrange
        var client = A.Fake<IFitbitApiClient>();
        var cache = new MemoryCache(new MemoryCacheOptions());
        var options = new DbContextOptionsBuilder<TomeshelfFitbitDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        await using var dbContext = new TomeshelfFitbitDbContext(options);
        await dbContext.Database.EnsureCreatedAsync();

        var date = DateOnly.FromDateTime(new DateTime(2025, 10, 16));

        A.CallTo(() => client.GetActivitiesAsync(date, A<CancellationToken>._))
         .Returns(Task.FromResult<ActivitiesResponse?>(new ActivitiesResponse
         {
             Summary = new ActivitiesResponse.ActivitiesSummary
             {
                 CaloriesOut = 2000,
                 Steps = 5000,
                 Floors = 10,
                 Distances = new List<ActivitiesResponse.ActivityDistance>
                 {
                     new() { Activity = "total", Distance = 3.5 }
                 }
             }
         }));

        A.CallTo(() => client.GetCaloriesInAsync(date, A<CancellationToken>._))
         .Returns(Task.FromResult<FoodLogSummaryResponse?>(new FoodLogSummaryResponse
         {
             Summary = new FoodLogSummaryResponse.FoodSummary
             {
                 Calories = 2100
             }
         }));

        A.CallTo(() => client.GetWeightAsync(date, A<int>._, A<CancellationToken>._))
         .Returns(Task.FromResult<WeightResponse?>(new WeightResponse()));

        A.CallTo(() => client.GetSleepAsync(date, A<CancellationToken>._))
         .Returns(Task.FromResult<SleepResponse?>(new SleepResponse
         {
             Entries = new List<SleepResponse.SleepEntry>
             {
                 new()
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
        var dashboard = await service.GetDashboardAsync(date, forceRefresh: true, CancellationToken.None);

        // Assert
        Assert.NotNull(dashboard);
        Assert.Equal("22:15", dashboard.Sleep.Bedtime);
        Assert.Equal("06:45", dashboard.Sleep.WakeTime);
    }

    [Fact]
    public async Task GetDashboardAsync_ForceRefreshBypassesCacheAndRefetches()
    {
        // Arrange
        var client = A.Fake<IFitbitApiClient>();
        var cache = new MemoryCache(new MemoryCacheOptions());
        var options = new DbContextOptionsBuilder<TomeshelfFitbitDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        await using var dbContext = new TomeshelfFitbitDbContext(options);
        await dbContext.Database.EnsureCreatedAsync();

        var date = DateOnly.FromDateTime(new DateTime(2025, 10, 15));
        var fetchCount = 0;

        A.CallTo(() => client.GetActivitiesAsync(date, A<CancellationToken>._))
         .ReturnsLazily(_ =>
         {
             fetchCount++;
             return Task.FromResult<ActivitiesResponse?>(new ActivitiesResponse
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
         .Returns(Task.FromResult<FoodLogSummaryResponse?>(new FoodLogSummaryResponse()));

        A.CallTo(() => client.GetWeightAsync(date, A<int>._, A<CancellationToken>._))
         .Returns(Task.FromResult<WeightResponse?>(new WeightResponse()));

        A.CallTo(() => client.GetSleepAsync(date, A<CancellationToken>._))
         .Returns(Task.FromResult<SleepResponse?>(new SleepResponse()));

        var service = new FitbitDashboardService(client, cache, dbContext, NullLogger<FitbitDashboardService>.Instance);

        // Act & Assert
        var first = await service.GetDashboardAsync(date, forceRefresh: true, CancellationToken.None);
        Assert.Equal(1, fetchCount);
        Assert.Equal(1, first?.Activity.Steps);

        var second = await service.GetDashboardAsync(date, forceRefresh: false, CancellationToken.None);
        Assert.Equal(1, fetchCount); // cached result, no new fetch
        Assert.Equal(1, second?.Activity.Steps);

        var third = await service.GetDashboardAsync(date, forceRefresh: true, CancellationToken.None);
        Assert.Equal(2, fetchCount); // force refresh triggered new fetch
        Assert.Equal(2, third?.Activity.Steps);
    }
}
