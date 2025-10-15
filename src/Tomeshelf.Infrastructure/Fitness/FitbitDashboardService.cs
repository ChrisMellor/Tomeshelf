#nullable enable
using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Tomeshelf.Application.Contracts;
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
    public async Task<FitbitDashboardDto?> GetDashboardAsync(DateOnly date, bool forceRefresh = false, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"fitbit:dashboard:{date:yyyy-MM-dd}";
        if (_cache.TryGetValue(cacheKey, out FitbitDashboardDto? cached) && cached is not null)
        {
            return cached;
        }

        var isToday = date == DateOnly.FromDateTime(DateTime.Today);
        var existing = await _dbContext.DailySnapshots.AsNoTracking()
                                       .SingleOrDefaultAsync(s => s.Date == date, cancellationToken)
                                       .ConfigureAwait(false);

        var shouldFetch = forceRefresh || existing is null || (isToday && ((DateTimeOffset.UtcNow - existing.GeneratedUtc) >= TodayRefreshThreshold));

        FitbitDashboardDto? snapshot = null;

        if (shouldFetch)
        {
            try
            {
                snapshot = await FetchSnapshotAsync(date, cancellationToken)
                       .ConfigureAwait(false);
                await UpsertSnapshotAsync(snapshot, cancellationToken)
                       .ConfigureAwait(false);
            }
            catch (Exception ex)
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

    private async Task UpsertSnapshotAsync(FitbitDashboardDto snapshot, CancellationToken cancellationToken)
    {
        var date = DateOnly.ParseExact(snapshot.Date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
        var entity = await _dbContext.DailySnapshots.SingleOrDefaultAsync(s => s.Date == date, cancellationToken)
                                     .ConfigureAwait(false);

        if (entity is null)
        {
            entity = new FitbitDailySnapshot { Date = date };
            _dbContext.DailySnapshots.Add(entity);
        }

        entity.GeneratedUtc = snapshot.GeneratedUtc;
        entity.StartingWeightKg = snapshot.Weight.StartingWeightKg;
        entity.CurrentWeightKg = snapshot.Weight.CurrentWeightKg;
        entity.ChangeWeightKg = snapshot.Weight.ChangeKg;
        entity.IntakeCalories = snapshot.Calories.IntakeCalories;
        entity.BurnedCalories = snapshot.Calories.BurnedCalories;
        entity.NetCalories = snapshot.Calories.NetCalories;
        entity.TotalSleepHours = snapshot.Sleep.TotalSleepHours;
        entity.TotalAwakeHours = snapshot.Sleep.TotalAwakeHours;
        entity.SleepEfficiencyPercentage = snapshot.Sleep.EfficiencyPercentage;
        entity.Bedtime = snapshot.Sleep.Bedtime;
        entity.WakeTime = snapshot.Sleep.WakeTime;
        entity.Steps = snapshot.Activity.Steps;
        entity.DistanceKm = snapshot.Activity.DistanceKm;
        entity.Floors = snapshot.Activity.Floors;

        await _dbContext.SaveChangesAsync(cancellationToken)
                        .ConfigureAwait(false);
    }

    private static FitbitDashboardDto MapSnapshot(FitbitDailySnapshot entity)
    {
        return new FitbitDashboardDto
        {
                Date = entity.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                GeneratedUtc = entity.GeneratedUtc,
                Weight = new FitbitWeightSummaryDto(entity.StartingWeightKg, entity.CurrentWeightKg, entity.ChangeWeightKg),
                Calories = new FitbitCaloriesSummaryDto(entity.IntakeCalories, entity.BurnedCalories, entity.NetCalories),
                Sleep = new FitbitSleepSummaryDto(entity.TotalSleepHours, entity.TotalAwakeHours, entity.SleepEfficiencyPercentage, entity.Bedtime, entity.WakeTime),
                Activity = new FitbitActivitySummaryDto(entity.Steps, entity.DistanceKm, entity.Floors)
        };
    }

    private static FitbitWeightSummaryDto BuildWeightSummary(WeightResponse? response)
    {
        if (response?.Entries is not
            {
                    Count: > 0
            })
        {
            return new FitbitWeightSummaryDto(null, null, null);
        }

        var data = response.Entries.Where(e => e.Weight.HasValue)
                           .Select(e =>
                            {
                                var timestamp = ParseDateTime(e.Date, e.Time) ?? DateTimeOffset.MinValue;

                                return new
                                {
                                        Timestamp = timestamp,
                                        e.Weight
                                };
                            })
                           .Where(e => e.Weight.HasValue)
                           .OrderBy(e => e.Timestamp)
                           .ToList();

        if (data.Count == 0)
        {
            return new FitbitWeightSummaryDto(null, null, null);
        }

        var starting = data.First()
                           .Weight;
        var current = data.Last()
                          .Weight;
        double? change = null;

        if (starting.HasValue && current.HasValue)
        {
            change = starting.Value - current.Value;
        }

        return new FitbitWeightSummaryDto(starting, current, change);
    }

    private static FitbitCaloriesSummaryDto BuildCaloriesSummary(CaloriesInResponse? calories, ActivitiesResponse? activities)
    {
        int? intake = null;

        var entry = calories?.Entries?.FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(entry?.Value) && int.TryParse(entry.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedIntake))
        {
            intake = parsedIntake;
        }

        var burned = activities?.Summary?.CaloriesOut;

        int? net = null;
        if (intake.HasValue && burned.HasValue)
        {
            net = intake.Value - burned.Value;
        }

        return new FitbitCaloriesSummaryDto(intake, burned, net);
    }

    private static FitbitSleepSummaryDto BuildSleepSummary(SleepResponse? response)
    {
        if (response?.Entries is not
            {
                    Count: > 0
            })
        {
            return new FitbitSleepSummaryDto(null, null, null, null, null);
        }

        var entries = response.Entries.Where(e => e.MinutesAsleep.HasValue || e.MinutesAwake.HasValue)
                              .ToList();

        if (entries.Count == 0)
        {
            return new FitbitSleepSummaryDto(null, null, null, null, null);
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

        double? efficiency = null;
        if (efficiencyValues.Count > 0)
        {
            efficiency = efficiencyValues.Average();
        }

        static double? ToHours(int minutes)
        {
            return minutes > 0
                    ? Math.Round(minutes / 60d, 2)
                    : null;
        }

        return new FitbitSleepSummaryDto(ToHours(totalSleepMinutes) ?? 0d, ToHours(totalAwakeMinutes), efficiency, bedTime?.ToString("HH:mm"), wakeTime?.ToString("HH:mm"));
    }

    private static FitbitActivitySummaryDto BuildActivitySummary(ActivitiesResponse? response)
    {
        if (response?.Summary is null)
        {
            return new FitbitActivitySummaryDto(null, null, null);
        }

        var distance = response.Summary.Distances?.FirstOrDefault(d => string.Equals(d.Activity, "total", StringComparison.OrdinalIgnoreCase))
                              ?.Distance;

        return new FitbitActivitySummaryDto(response.Summary.Steps, distance, response.Summary.Floors);
    }

    private static DateTimeOffset? ParseDateTime(string? date, string? time)
    {
        if (string.IsNullOrWhiteSpace(date) && string.IsNullOrWhiteSpace(time))
        {
            return null;
        }

        var composite = !string.IsNullOrWhiteSpace(time)
                ? $"{date}T{time}"
                : date;

        if (string.IsNullOrWhiteSpace(composite))
        {
            return null;
        }

        if (DateTimeOffset.TryParse(composite, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var dto))
        {
            return dto;
        }

        if (DateTime.TryParse(composite, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var dt))
        {
            return new DateTimeOffset(dt);
        }

        return null;
    }
}