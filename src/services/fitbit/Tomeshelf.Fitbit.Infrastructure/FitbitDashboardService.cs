using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Tomeshelf.Fitbit.Application;
using Tomeshelf.Fitbit.Application.Abstractions.Services;
using Tomeshelf.Fitbit.Application.Exceptions;
using Tomeshelf.Fitbit.Domain;
using Tomeshelf.Fitbit.Infrastructure.Models;

namespace Tomeshelf.Fitbit.Infrastructure;

/// <summary>
///     Provides cached Fitbit dashboard snapshots backed by persisted daily data.
/// </summary>
public sealed class FitbitDashboardService : IFitbitDashboardService
{
    private const int WeightLookbackDays = 1;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan TodayRefreshThreshold = TimeSpan.FromMinutes(15);

    private readonly IMemoryCache _cache;
    private readonly IFitbitApiClient _client;
    private readonly TomeshelfFitbitDbContext _dbContext;
    private readonly ILogger<FitbitDashboardService> _logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="FitbitDashboardService" /> class.
    /// </summary>
    /// <param name="client">The client.</param>
    /// <param name="cache">The cache.</param>
    /// <param name="dbContext">The db context.</param>
    /// <param name="logger">The logger.</param>
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
        DashboardFetchResult fetchResult = null;

        if (shouldFetch)
        {
            try
            {
                fetchResult = await FetchSnapshotAsync(date, existing, cancellationToken)
                   .ConfigureAwait(false);
                snapshot = fetchResult.Snapshot;
                await UpsertSnapshotAsync(snapshot, cancellationToken, fetchResult.Status)
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
            snapshot = await ApplyWeightFallbackAsync(date, snapshot, cancellationToken)
               .ConfigureAwait(false);
            _cache.Set(cacheKey, snapshot, CacheDuration);
        }

        return snapshot;
    }

    /// <summary>
    ///     Applies the weight fallback asynchronously.
    /// </summary>
    /// <param name="date">The date.</param>
    /// <param name="summary">The summary.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the operation result.</returns>
    private async Task<FitbitWeightSummaryDto> ApplyWeightFallbackAsync(DateOnly date, FitbitWeightSummaryDto summary, CancellationToken cancellationToken)
    {
        if (HasWeightData(summary))
        {
            return summary;
        }

        var fallback = await _dbContext.DailySnapshots
                                       .AsNoTracking()
                                       .Where(snapshot => (snapshot.Date < date) && (snapshot.CurrentWeightKg.HasValue || snapshot.StartingWeightKg.HasValue))
                                       .OrderByDescending(snapshot => snapshot.Date)
                                       .Select(snapshot => new
                                        {
                                            snapshot.CurrentWeightKg,
                                            snapshot.StartingWeightKg,
                                            snapshot.BodyFatPercentage,
                                            snapshot.LeanMassKg
                                        })
                                       .FirstOrDefaultAsync(cancellationToken)
                                       .ConfigureAwait(false);

        var weight = fallback?.CurrentWeightKg ?? fallback?.StartingWeightKg;
        if (!weight.HasValue)
        {
            return summary;
        }

        return new FitbitWeightSummaryDto
        {
            StartingWeightKg = weight,
            CurrentWeightKg = weight,
            BodyFatPercentage = fallback?.BodyFatPercentage,
            LeanMassKg = fallback?.LeanMassKg
        };
    }

    /// <summary>
    ///     Applies the weight fallback asynchronously.
    /// </summary>
    /// <param name="date">The date.</param>
    /// <param name="snapshot">The snapshot.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the operation result.</returns>
    private async Task<FitbitDashboardDto> ApplyWeightFallbackAsync(DateOnly date, FitbitDashboardDto snapshot, CancellationToken cancellationToken)
    {
        if (snapshot is null)
        {
            return null;
        }

        var updatedWeight = await ApplyWeightFallbackAsync(date, snapshot.Weight, cancellationToken)
           .ConfigureAwait(false);

        if (ReferenceEquals(updatedWeight, snapshot.Weight))
        {
            return snapshot;
        }

        return snapshot with { Weight = updatedWeight };
    }

    /// <summary>
    ///     Builds the activity summary.
    /// </summary>
    /// <param name="response">The response.</param>
    /// <returns>The result of the operation.</returns>
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

    /// <summary>
    ///     Builds the calories summary.
    /// </summary>
    /// <param name="foodLog">The food log.</param>
    /// <param name="activities">The activities.</param>
    /// <returns>The result of the operation.</returns>
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

    /// <summary>
    ///     Builds the sleep summary.
    /// </summary>
    /// <param name="response">The response.</param>
    /// <returns>The result of the operation.</returns>
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

    /// <summary>
    ///     Builds the weight summary.
    /// </summary>
    /// <param name="response">The response.</param>
    /// <returns>The result of the operation.</returns>
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

    /// <summary>
    ///     Fetchs the snapshot asynchronously.
    /// </summary>
    /// <param name="date">The date.</param>
    /// <param name="existingSnapshot">The existing snapshot.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the operation result.</returns>
    private async Task<DashboardFetchResult> FetchSnapshotAsync(DateOnly date, FitbitDailySnapshot existingSnapshot, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching Fitbit data from API for {Date}", date);

        var activitiesTask = TryFetchAsync("activities", () => _client.GetActivitiesAsync(date, cancellationToken), date, cancellationToken);
        var caloriesTask = TryFetchAsync("calories", () => _client.GetCaloriesInAsync(date, cancellationToken), date, cancellationToken);
        var sleepTask = TryFetchAsync("sleep", () => _client.GetSleepAsync(date, cancellationToken), date, cancellationToken);
        var weightTask = TryFetchAsync("weight", () => _client.GetWeightAsync(date, WeightLookbackDays, cancellationToken), date, cancellationToken);

        await Task.WhenAll(activitiesTask, caloriesTask, sleepTask, weightTask)
                  .ConfigureAwait(false);

        var activitiesResult = await activitiesTask.ConfigureAwait(false);
        var caloriesResult = await caloriesTask.ConfigureAwait(false);
        var sleepResult = await sleepTask.ConfigureAwait(false);
        var weightResult = await weightTask.ConfigureAwait(false);

        if (!activitiesResult.IsSuccess && !caloriesResult.IsSuccess && !sleepResult.IsSuccess && !weightResult.IsSuccess)
        {
            throw GetFirstFailure(activitiesResult, caloriesResult, sleepResult, weightResult);
        }

        var fallback = existingSnapshot is null
            ? null
            : MapSnapshot(existingSnapshot);

        var activitySummary = activitiesResult.IsSuccess
            ? BuildActivitySummary(activitiesResult.Value)
            : fallback?.Activity ?? new FitbitActivitySummaryDto(null, null, null);

        var caloriesSummary = caloriesResult.IsSuccess
            ? BuildCaloriesSummary(caloriesResult.Value, activitiesResult.Value)
            : fallback?.Calories ?? new FitbitCaloriesSummaryDto();

        var sleepSummary = sleepResult.IsSuccess
            ? BuildSleepSummary(sleepResult.Value)
            : fallback?.Sleep ?? new FitbitSleepSummaryDto();

        var weightSummary = weightResult.IsSuccess
            ? BuildWeightSummary(weightResult.Value)
            : fallback?.Weight ?? new FitbitWeightSummaryDto();
        var weightHasData = weightResult.IsSuccess && HasWeightData(weightSummary);

        if (!weightHasData)
        {
            weightSummary = await ApplyWeightFallbackAsync(date, weightSummary, cancellationToken)
               .ConfigureAwait(false);
        }

        var snapshot = new FitbitDashboardDto
        {
            Date = date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            GeneratedUtc = DateTimeOffset.UtcNow,
            Weight = weightSummary,
            Calories = caloriesSummary,
            Sleep = sleepSummary,
            Activity = activitySummary
        };

        var status = new FetchStatus(activitiesResult.IsSuccess, caloriesResult.IsSuccess, sleepResult.IsSuccess, weightHasData);

        return new DashboardFetchResult(snapshot, status);
    }

    /// <summary>
    ///     Gets the first failure.
    /// </summary>
    /// <typeparam name="T1">The type of T1.</typeparam>
    /// <typeparam name="T2">The type of T2.</typeparam>
    /// <typeparam name="T3">The type of T3.</typeparam>
    /// <typeparam name="T4">The type of T4.</typeparam>
    /// <param name="activities">The activities.</param>
    /// <param name="calories">The calories.</param>
    /// <param name="sleep">The sleep.</param>
    /// <param name="weight">The weight.</param>
    /// <returns>The result of the operation.</returns>
    private static Exception GetFirstFailure<T1, T2, T3, T4>(FetchResult<T1> activities, FetchResult<T2> calories, FetchResult<T3> sleep, FetchResult<T4> weight)
    {
        return activities.Exception ?? calories.Exception ?? sleep.Exception ?? weight.Exception ?? new InvalidOperationException("Failed to fetch Fitbit data.");
    }

    /// <summary>
    ///     Determines whether the specified summary has weight data.
    /// </summary>
    /// <param name="summary">The summary.</param>
    /// <returns>True if the condition is met; otherwise, false.</returns>
    private static bool HasWeightData(FitbitWeightSummaryDto summary)
    {
        if (summary is null)
        {
            return false;
        }

        return summary.CurrentWeightKg.HasValue || summary.StartingWeightKg.HasValue || summary.ChangeKg.HasValue || summary.BodyFatPercentage.HasValue || summary.LeanMassKg.HasValue;
    }

    /// <summary>
    ///     Determines whether the specified exception is a cancellation.
    /// </summary>
    /// <param name="exception">The exception.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>True if the condition is met; otherwise, false.</returns>
    private static bool IsCancellation(Exception exception, CancellationToken cancellationToken)
    {
        return exception is OperationCanceledException && cancellationToken.IsCancellationRequested;
    }

    /// <summary>
    ///     Determines whether the specified exception is a critical fitbit failure.
    /// </summary>
    /// <param name="exception">The exception.</param>
    /// <returns>True if the condition is met; otherwise, false.</returns>
    private static bool IsCriticalFitbitFailure(Exception exception)
    {
        return exception is InvalidOperationException or FitbitRateLimitExceededException or FitbitBadRequestException or HttpRequestException;
    }

    /// <summary>
    ///     Maps the snapshot.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <returns>The result of the operation.</returns>
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

    /// <summary>
    ///     Parses the date time.
    /// </summary>
    /// <param name="date">The date.</param>
    /// <param name="time">The time.</param>
    /// <returns>The result of the operation.</returns>
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

    /// <summary>
    ///     Attempts to fetch asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of t.</typeparam>
    /// <param name="metric">The metric.</param>
    /// <param name="fetch">The fetch.</param>
    /// <param name="date">The date.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the operation result.</returns>
    private async Task<FetchResult<T>> TryFetchAsync<T>(string metric, Func<Task<T>> fetch, DateOnly date, CancellationToken cancellationToken)
    {
        try
        {
            var result = await fetch()
               .ConfigureAwait(false);

            return FetchResult<T>.Success(result);
        }
        catch (Exception ex) when (!IsCancellation(ex, cancellationToken))
        {
            _logger.LogWarning(ex, "Failed to fetch Fitbit {Metric} data for {Date}.", metric, date);

            return FetchResult<T>.Failure(ex);
        }
    }

    /// <summary>
    ///     Inserts or updates the snapshot asynchronously.
    /// </summary>
    /// <param name="snapshot">The snapshot.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="status">The status.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task UpsertSnapshotAsync(FitbitDashboardDto snapshot, CancellationToken cancellationToken, FetchStatus status)
    {
        var date = DateOnly.ParseExact(snapshot.Date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
        var entity = await _dbContext.DailySnapshots
                                     .SingleOrDefaultAsync(s => s.Date == date, cancellationToken)
                                     .ConfigureAwait(false);

        if (entity is null)
        {
            entity = new FitbitDailySnapshot { Date = date };
            _dbContext.DailySnapshots.Add(entity);
        }

        entity.GeneratedUtc = snapshot.GeneratedUtc;

        if (status.Weight)
        {
            entity.StartingWeightKg = snapshot.Weight.StartingWeightKg;
            entity.CurrentWeightKg = snapshot.Weight.CurrentWeightKg;
            entity.ChangeWeightKg = snapshot.Weight.ChangeKg;
            entity.BodyFatPercentage = snapshot.Weight.BodyFatPercentage;
            entity.LeanMassKg = snapshot.Weight.LeanMassKg;
        }

        if (status.Calories)
        {
            entity.IntakeCalories = snapshot.Calories.IntakeCalories;
            entity.BurnedCalories = snapshot.Calories.BurnedCalories;
            entity.NetCalories = snapshot.Calories.NetCalories;
            entity.CarbsGrams = snapshot.Calories.CarbsGrams;
            entity.FatGrams = snapshot.Calories.FatGrams;
            entity.FiberGrams = snapshot.Calories.FiberGrams;
            entity.ProteinGrams = snapshot.Calories.ProteinGrams;
            entity.SodiumMilligrams = snapshot.Calories.SodiumMilligrams;
        }

        if (status.Sleep)
        {
            var levels = snapshot.Sleep.Levels ?? new FitbitSleepLevelsDto();
            entity.TotalSleepHours = snapshot.Sleep.TotalSleepHours;
            entity.TotalAwakeHours = snapshot.Sleep.TotalAwakeHours;
            entity.SleepEfficiencyPercentage = snapshot.Sleep.EfficiencyPercentage;
            entity.Bedtime = snapshot.Sleep.Bedtime;
            entity.WakeTime = snapshot.Sleep.WakeTime;
            entity.SleepDeepMinutes = levels.DeepMinutes;
            entity.SleepLightMinutes = levels.LightMinutes;
            entity.SleepRemMinutes = levels.RemMinutes;
            entity.SleepWakeMinutes = levels.WakeMinutes;
        }

        if (status.Activities)
        {
            entity.Steps = snapshot.Activity.Steps;
            entity.DistanceKm = snapshot.Activity.DistanceKm;
            entity.Floors = snapshot.Activity.Floors;
        }

        await _dbContext.SaveChangesAsync(cancellationToken)
                        .ConfigureAwait(false);
    }
}