using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tomeshelf.Fitbit.Application.Abstractions.Services;
using Tomeshelf.Fitbit.Application.Features.Overview.Models;

namespace Tomeshelf.Fitbit.Infrastructure;

public sealed class FitbitOverviewService : IFitbitOverviewService
{
    private readonly IFitbitDashboardService _dashboardService;
    private readonly TomeshelfFitbitDbContext _dbContext;
    private readonly ILogger<FitbitOverviewService> _logger;

    public FitbitOverviewService(IFitbitDashboardService dashboardService, TomeshelfFitbitDbContext dbContext, ILogger<FitbitOverviewService> logger)
    {
        _dashboardService = dashboardService;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<FitbitOverviewDto> GetOverviewAsync(DateOnly date, bool forceRefresh, CancellationToken cancellationToken)
    {
        var daily = await _dashboardService.GetDashboardAsync(date, forceRefresh, cancellationToken)
                                           .ConfigureAwait(false);
        if (daily is null)
        {
            return null;
        }

        var last7 = await BuildRangeAsync(date, 7, cancellationToken)
           .ConfigureAwait(false);
        var last30 = await BuildRangeAsync(date, 30, cancellationToken)
           .ConfigureAwait(false);

        return new FitbitOverviewDto
        {
            Daily = daily,
            Last7Days = last7,
            Last30Days = last30
        };
    }

    private async Task<FitbitOverviewRangeDto> BuildRangeAsync(DateOnly date, int days, CancellationToken cancellationToken)
    {
        if (days <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(days), "Range days must be greater than zero.");
        }

        var start = date.AddDays(-(days - 1));

        var snapshots = await _dbContext.DailySnapshots
                                        .AsNoTracking()
                                        .Where(snapshot => (snapshot.Date >= start) && (snapshot.Date <= date))
                                        .ToListAsync(cancellationToken)
                                        .ConfigureAwait(false);

        var snapshotLookup = snapshots.ToDictionary(snapshot => snapshot.Date, snapshot => snapshot);

        var lastKnownWeight = await GetLastKnownWeightAsync(start, cancellationToken)
           .ConfigureAwait(false);

        var items = new List<FitbitOverviewDayDto>(days);

        for (var offset = 0; offset < days; offset++)
        {
            var currentDate = start.AddDays(offset);
            snapshotLookup.TryGetValue(currentDate, out var snapshot);

            var weight = snapshot?.CurrentWeightKg ?? snapshot?.StartingWeightKg;
            if (weight.HasValue)
            {
                lastKnownWeight = weight;
            }
            else if (lastKnownWeight.HasValue)
            {
                weight = lastKnownWeight;
            }

            items.Add(new FitbitOverviewDayDto
            {
                Date = currentDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                WeightKg = weight,
                Steps = snapshot?.Steps,
                SleepHours = snapshot?.TotalSleepHours,
                NetCalories = snapshot?.NetCalories
            });
        }

        _logger.LogInformation("Built Fitbit overview range for {Start} - {End} ({Days} days)", start, date, days);

        return new FitbitOverviewRangeDto
        {
            Days = days,
            Items = items
        };
    }

    private async Task<double?> GetLastKnownWeightAsync(DateOnly startDate, CancellationToken cancellationToken)
    {
        var previous = await _dbContext.DailySnapshots
                                       .AsNoTracking()
                                       .Where(snapshot => (snapshot.Date < startDate) && (snapshot.CurrentWeightKg.HasValue || snapshot.StartingWeightKg.HasValue))
                                       .OrderByDescending(snapshot => snapshot.Date)
                                       .Select(snapshot => new
                                        {
                                            snapshot.CurrentWeightKg,
                                            snapshot.StartingWeightKg
                                        })
                                       .FirstOrDefaultAsync(cancellationToken)
                                       .ConfigureAwait(false);

        return previous?.CurrentWeightKg ?? previous?.StartingWeightKg;
    }
}