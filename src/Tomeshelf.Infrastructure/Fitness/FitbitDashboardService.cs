using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Tomeshelf.Domain.Entities.Fitness;
using Tomeshelf.Infrastructure.Fitness.Models;
using Tomeshelf.Infrastructure.Persistence;

namespace Tomeshelf.Infrastructure.Fitness;

/// <summary>
///     Provides cached Fitbit dashboard snapshots backed by persisted daily data.
/// </summary>
public sealed class FitbitDashboardService
{
    private const int WeightLookbackDays = 1;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan TodayRefreshThreshold = TimeSpan.FromMinutes(15);

    private readonly IMemoryCache _cache;
    private readonly IFitbitApiClient _client;
    private readonly TomeshelfFitbitDbContext _dbContext;
    private readonly ILogger<FitbitDashboardService> _logger;

    public FitbitDashboardService(IFitbitApiClient client, IMemoryCache cache, TomeshelfFitbitDbContext dbContext, ILogger<FitbitDashboardService> logger)
    {
        _client = client;
        _cache = cache;
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    ///     Retrieves a dashboard snapshot for the supplied date, preferring stored data.
    /// </summary>
    public async Task<FitbitDashboardDto> GetDashboardAsync(DateOnly date, bool forceRefresh = false, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"fitbit:dashboard:{date:yyyy-MM-dd}";
        if (!forceRefresh && _cache.TryGetValue(cacheKey, out FitbitDashboardDto cached) && cached is not null)
        {
            return cached;
        }

        var isToday = date == DateOnly.FromDateTime(DateTime.Today);
        var existing = await _dbContext.DailySnapshots
                                       .AsNoTracking()
                                       .SingleOrDefaultAsync(s => s.Date == date, cancellationToken)
                                       .ConfigureAwait(false);

        var shouldFetch = forceRefresh || existing is null || (isToday && ((DateTimeOffset.UtcNow - existing.GeneratedUtc) >= TodayRefreshThreshold));

        FitbitDashboardDto snapshot = null;

        if (shouldFetch)
        {
            try
            {
                snapshot = await FetchSnapshotAsync(date, cancellationToken)
                   .ConfigureAwait(false);
                await UpsertSnapshotAsync(snapshot, cancellationToken)
                   .ConfigureAwait(false);
            }
            catch (Exception ex) when (!IsCriticalFitbitFailure(ex))
            {
                _logger.LogWarning(ex, "Failed to refresh Fitbit snapshot for {Date}", date);
            }
        }

        if (snapshot is null && existing is not null)
        {
            snapshot = MapSnapshot(existing);
        }

        if (snapshot is not null)
        {
            _cache.Set(cacheKey, snapshot, CacheDuration);
        }

        return snapshot;
    }

    private static FitbitActivitySummaryDto BuildActivitySummary(ActivitiesResponse response)
    {
        if (response?.Summary is null)
        {
            return new FitbitActivitySummaryDto(null, null, null);
        }

        var distance = response.Summary.Distances?.FirstOrDefault(d => string.Equals(d.Activity, "total", StringComparison.OrdinalIgnoreCase))
                              ?.Distance;

        return new FitbitActivitySummaryDto(response.Summary.Steps, distance, response.Summary.Floors);
    }

    private static FitbitCaloriesSummaryDto BuildCaloriesSummary(FoodLogSummaryResponse foodLog, ActivitiesResponse activities)
    {
        var summary = foodLog?.Summary;
        var intake = summary?.Calories;
        var burned = activities?.Summary?.CaloriesOut;

        int? net = null;
        if (intake.HasValue && burned.HasValue)
        {
            net = intake.Value - burned.Value;
        }

        return new FitbitCaloriesSummaryDto
        {
            IntakeCalories = intake,
            BurnedCalories = burned,
            NetCalories = net,
            CarbsGrams = summary?.Carbs,
            FatGrams = summary?.Fat,
            FiberGrams = summary?.Fiber,
            ProteinGrams = summary?.Protein,
            SodiumMilligrams = summary?.Sodium
        };
    }

    private static FitbitSleepSummaryDto BuildSleepSummary(SleepResponse response)
    {
        if (response?.Entries is not
            {
                Count: > 0
            })
        {
            return new FitbitSleepSummaryDto();
        }

        var entries = response.Entries
                              .Where(e => e.MinutesAsleep.HasValue || e.MinutesAwake.HasValue)
                              .ToList();

        if (entries.Count == 0)
        {
            return new FitbitSleepSummaryDto();
        }

        var totalSleepMinutes = entries.Sum(e => e.MinutesAsleep ?? 0);
        var totalAwakeMinutes = entries.Sum(e => (e.MinutesAwake ?? 0) + (e.MinutesAfterWakeup ?? 0) + (e.MinutesToFallAsleep ?? 0));
        var efficiencyValues = entries.Select(e => e.Efficiency)
                                      .Where(efficiency => efficiency.HasValue)
                                      .Select(efficiency => (double)efficiency.Value)
                                      .ToList();

        var bedTime = entries.Select(e => ParseDateTime(e.DateOfSleep, e.StartTime))
                             .Where(d => d.HasValue)
                             .OrderBy(d => d)
                             .FirstOrDefault();
        var wakeTime = entries.Select(e => ParseDateTime(e.DateOfSleep, e.EndTime))
                              .Where(d => d.HasValue)
                              .OrderByDescending(d => d)
                              .FirstOrDefault();

        double? efficiency = efficiencyValues.Count > 0
            ? efficiencyValues.Average()
            : null;

        static double? ToHours(int minutes)
        {
            return minutes > 0 ? Math.Round(minutes / 60d, 2) :
                minutes == 0 ? 0d : null;
        }

        int? SumLevelMinutes(Func<SleepResponse.SleepLevelSummary, SleepResponse.SleepLevelSummaryItem> selector)
        {
            var total = 0;
            var hasData = false;

            foreach (var entry in entries)
            {
                var summary = entry.Levels?.Summary;
                if (summary is null)
                {
                    continue;
                }

                var minutes = selector(summary)
                  ?.Minutes;
                if (minutes.HasValue)
                {
                    total += minutes.Value;
                    hasData = true;
                }
            }

            return hasData
                ? total
                : null;
        }

        var levels = new FitbitSleepLevelsDto
        {
            DeepMinutes = SumLevelMinutes(s => s.Deep),
            LightMinutes = SumLevelMinutes(s => s.Light),
            RemMinutes = SumLevelMinutes(s => s.Rem),
            WakeMinutes = SumLevelMinutes(s => s.Wake)
        };

        return new FitbitSleepSummaryDto
        {
            TotalSleepHours = ToHours(totalSleepMinutes),
            TotalAwakeHours = ToHours(totalAwakeMinutes),
            EfficiencyPercentage = efficiency,
            Bedtime = bedTime?.ToString("HH:mm"),
            WakeTime = wakeTime?.ToString("HH:mm"),
            Levels = levels
        };
    }

    private static FitbitWeightSummaryDto BuildWeightSummary(WeightResponse response)
    {
        if (response?.Entries is not
            {
                Count: > 0
            })
        {
            return new FitbitWeightSummaryDto();
        }

        var data = response.Entries
                           .Where(e => e.Weight.HasValue)
                           .Select(entry =>
                            {
                                var timestamp = ParseDateTime(entry.Date, entry.Time) ?? DateTimeOffset.MinValue;

                                return new
                                {
                                    Timestamp = timestamp,
                                    entry.Weight,
                                    entry.BodyFatPercentage,
                                    entry.LeanMassKg
                                };
                            })
                           .Where(e => e.Weight.HasValue)
                           .OrderBy(e => e.Timestamp)
                           .ToList();

        if (data.Count == 0)
        {
            return new FitbitWeightSummaryDto();
        }

        var startingWeight = data.First()
                                 .Weight;
        var current = data.Last();
        var currentWeight = current.Weight;

        double? change = null;
        if (startingWeight.HasValue && currentWeight.HasValue)
        {
            change = startingWeight.Value - currentWeight.Value;
        }

        var bodyFat = current.BodyFatPercentage;
        var leanMass = current.LeanMassKg;

        if (currentWeight.HasValue)
        {
            var weight = currentWeight.Value;

            if (!leanMass.HasValue && bodyFat.HasValue)
            {
                leanMass = Math.Round(weight * (1 - (bodyFat.Value / 100d)), 2);
            }

            if (!bodyFat.HasValue && leanMass.HasValue && (weight > 0))
            {
                bodyFat = Math.Round((1 - (leanMass.Value / weight)) * 100d, 2);
            }
        }

        return new FitbitWeightSummaryDto
        {
            StartingWeightKg = startingWeight,
            CurrentWeightKg = currentWeight,
            ChangeKg = change,
            BodyFatPercentage = bodyFat,
            LeanMassKg = leanMass
        };
    }

    private async Task<FitbitDashboardDto> FetchSnapshotAsync(DateOnly date, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching Fitbit data from API for {Date}", date);

        var activitiesTask = _client.GetActivitiesAsync(date, cancellationToken);
        var caloriesTask = _client.GetCaloriesInAsync(date, cancellationToken);
        var sleepTask = _client.GetSleepAsync(date, cancellationToken);
        var weightTask = _client.GetWeightAsync(date, WeightLookbackDays, cancellationToken);

        await Task.WhenAll(activitiesTask, caloriesTask, sleepTask, weightTask)
                  .ConfigureAwait(false);

        var activities = await activitiesTask.ConfigureAwait(false);
        var calories = await caloriesTask.ConfigureAwait(false);
        var sleep = await sleepTask.ConfigureAwait(false);
        var weight = await weightTask.ConfigureAwait(false);

        return new FitbitDashboardDto
        {
            Date = date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            GeneratedUtc = DateTimeOffset.UtcNow,
            Weight = BuildWeightSummary(weight),
            Calories = BuildCaloriesSummary(calories, activities),
            Sleep = BuildSleepSummary(sleep),
            Activity = BuildActivitySummary(activities)
        };
    }

    private static bool IsCriticalFitbitFailure(Exception exception)
    {
        return exception is InvalidOperationException or FitbitRateLimitExceededException or FitbitBadRequestException or HttpRequestException;
    }

    private static FitbitDashboardDto MapSnapshot(FitbitDailySnapshot entity)
    {
        return new FitbitDashboardDto
        {
            Date = entity.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            GeneratedUtc = entity.GeneratedUtc,
            Weight = new FitbitWeightSummaryDto
            {
                StartingWeightKg = entity.StartingWeightKg,
                CurrentWeightKg = entity.CurrentWeightKg,
                ChangeKg = entity.ChangeWeightKg,
                BodyFatPercentage = entity.BodyFatPercentage,
                LeanMassKg = entity.LeanMassKg
            },
            Calories = new FitbitCaloriesSummaryDto
            {
                IntakeCalories = entity.IntakeCalories,
                BurnedCalories = entity.BurnedCalories,
                NetCalories = entity.NetCalories,
                CarbsGrams = entity.CarbsGrams,
                FatGrams = entity.FatGrams,
                FiberGrams = entity.FiberGrams,
                ProteinGrams = entity.ProteinGrams,
                SodiumMilligrams = entity.SodiumMilligrams
            },
            Sleep = new FitbitSleepSummaryDto
            {
                TotalSleepHours = entity.TotalSleepHours,
                TotalAwakeHours = entity.TotalAwakeHours,
                EfficiencyPercentage = entity.SleepEfficiencyPercentage,
                Bedtime = entity.Bedtime,
                WakeTime = entity.WakeTime,
                Levels = new FitbitSleepLevelsDto
                {
                    DeepMinutes = entity.SleepDeepMinutes,
                    LightMinutes = entity.SleepLightMinutes,
                    RemMinutes = entity.SleepRemMinutes,
                    WakeMinutes = entity.SleepWakeMinutes
                }
            },
            Activity = new FitbitActivitySummaryDto(entity.Steps, entity.DistanceKm, entity.Floors)
        };
    }

    private static DateTimeOffset? ParseDateTime(string date, string time)
    {
        if (string.IsNullOrWhiteSpace(date) && string.IsNullOrWhiteSpace(time))
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(time))
        {
            if (DateTimeOffset.TryParse(time, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var parsedTimeOnly))
            {
                return parsedTimeOnly;
            }

            if (!string.IsNullOrWhiteSpace(date))
            {
                var composite = $"{date}T{time}";
                if (DateTimeOffset.TryParse(composite, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var dto))
                {
                    return dto;
                }

                if (DateTime.TryParse(composite, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var dt))
                {
                    return new DateTimeOffset(dt);
                }
            }
        }

        if (!string.IsNullOrWhiteSpace(date))
        {
            if (DateTimeOffset.TryParse(date, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var dtoDateOnly))
            {
                return dtoDateOnly;
            }

            if (DateTime.TryParse(date, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var dtDateOnly))
            {
                return new DateTimeOffset(dtDateOnly);
            }
        }

        return null;
    }

    private async Task UpsertSnapshotAsync(FitbitDashboardDto snapshot, CancellationToken cancellationToken)
    {
        var date = DateOnly.ParseExact(snapshot.Date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
        var entity = await _dbContext.DailySnapshots
                                     .SingleOrDefaultAsync(s => s.Date == date, cancellationToken)
                                     .ConfigureAwait(false);

        if (entity is null)
        {
            entity = new FitbitDailySnapshot
            {
                Date = date
            };
            _dbContext.DailySnapshots.Add(entity);
        }

        entity.GeneratedUtc = snapshot.GeneratedUtc;
        entity.StartingWeightKg = snapshot.Weight.StartingWeightKg;
        entity.CurrentWeightKg = snapshot.Weight.CurrentWeightKg;
        entity.ChangeWeightKg = snapshot.Weight.ChangeKg;
        entity.BodyFatPercentage = snapshot.Weight.BodyFatPercentage;
        entity.LeanMassKg = snapshot.Weight.LeanMassKg;
        entity.IntakeCalories = snapshot.Calories.IntakeCalories;
        entity.BurnedCalories = snapshot.Calories.BurnedCalories;
        entity.NetCalories = snapshot.Calories.NetCalories;
        entity.CarbsGrams = snapshot.Calories.CarbsGrams;
        entity.FatGrams = snapshot.Calories.FatGrams;
        entity.FiberGrams = snapshot.Calories.FiberGrams;
        entity.ProteinGrams = snapshot.Calories.ProteinGrams;
        entity.SodiumMilligrams = snapshot.Calories.SodiumMilligrams;
        entity.TotalSleepHours = snapshot.Sleep.TotalSleepHours;
        entity.TotalAwakeHours = snapshot.Sleep.TotalAwakeHours;
        entity.SleepEfficiencyPercentage = snapshot.Sleep.EfficiencyPercentage;
        entity.Bedtime = snapshot.Sleep.Bedtime;
        entity.WakeTime = snapshot.Sleep.WakeTime;
        entity.SleepDeepMinutes = snapshot.Sleep.Levels.DeepMinutes;
        entity.SleepLightMinutes = snapshot.Sleep.Levels.LightMinutes;
        entity.SleepRemMinutes = snapshot.Sleep.Levels.RemMinutes;
        entity.SleepWakeMinutes = snapshot.Sleep.Levels.WakeMinutes;
        entity.Steps = snapshot.Activity.Steps;
        entity.DistanceKm = snapshot.Activity.DistanceKm;
        entity.Floors = snapshot.Activity.Floors;

        await _dbContext.SaveChangesAsync(cancellationToken)
                        .ConfigureAwait(false);
    }
}