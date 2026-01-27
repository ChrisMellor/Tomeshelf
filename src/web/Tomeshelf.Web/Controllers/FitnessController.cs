using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Web.Models.Fitness;
using Tomeshelf.Web.Services;

namespace Tomeshelf.Web.Controllers;

[Route("fitness")]
public sealed class FitnessController : Controller
{
    private readonly IFitbitApi _fitbitApi;
    private readonly ILogger<FitnessController> _logger;

    public FitnessController(IFitbitApi fitbitApi, ILogger<FitnessController> logger)
    {
        _fitbitApi = fitbitApi;
        _logger = logger;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index([FromQuery] string date, [FromQuery] bool refresh = false, [FromQuery] string unit = null, CancellationToken cancellationToken = default)
    {
        var targetDate = ResolveDate(date);
        var today = DateOnly.FromDateTime(DateTime.Today);
        var shouldRefresh = refresh || (targetDate == today);
        var selectedUnit = WeightUnitConverter.Parse(unit);
        var unitQuery = WeightUnitConverter.ToQueryValue(selectedUnit);

        using var scope = _logger.BeginScope(new
        {
            Date = targetDate,
            Refresh = shouldRefresh
        });
        _logger.LogInformation("Rendering Fitbit dashboard for {Date} (Refresh: {Refresh})", targetDate, shouldRefresh);

        try
        {
            var dateParameter = targetDate.ToString("yyyy-MM-dd");
            var returnUrl = Url.ActionLink("Index", "Fitness", new { date = dateParameter, unit = unitQuery }) ?? $"{Request.Scheme}://{Request.Host}/fitness?date={dateParameter}&unit={unitQuery}";

            var overview = await _fitbitApi.GetOverviewAsync(dateParameter, shouldRefresh, returnUrl, cancellationToken);

            if (overview is null)
            {
                return View(FitnessDashboardViewModel.Empty(dateParameter, selectedUnit, "No Fitbit data is available for the selected date."));
            }

            var summary = CreateSummary(overview.Daily);
            var last7 = BuildRangeViewModel("Last 7 days", overview.Last7Days, selectedUnit);
            var last30 = BuildRangeViewModel("Last 30 days", overview.Last30Days, selectedUnit);
            var hasTrendData = last7.HasData || last30.HasData;

            var model = new FitnessDashboardViewModel
            {
                SelectedDate = dateParameter,
                TodayIso = today.ToString("yyyy-MM-dd"),
                PreviousDate = targetDate.AddDays(-1)
                                             .ToString("yyyy-MM-dd"),
                NextDate = targetDate < today
                            ? targetDate.AddDays(1)
                                        .ToString("yyyy-MM-dd")
                            : null,
                Unit = selectedUnit,
                Summary = summary,
                Last7Days = last7,
                Last30Days = last30,
                ErrorMessage = summary is null
                            ? hasTrendData
                                ? "No daily Fitbit data is available for the selected date."
                                : "No Fitbit data is available for the selected date."
                            : null
            };

            return View(model);
        }
        catch (FitbitAuthorizationRequiredException authEx)
        {
            _logger.LogInformation("Resolving Fitbit authorization flow for {Date}", targetDate);

            try
            {
                var authorizeUri = await _fitbitApi.ResolveAuthorizationAsync(authEx.Location, cancellationToken);

                return Redirect(authorizeUri.ToString());
            }
            catch (Exception resolveEx)
            {
                _logger.LogWarning(resolveEx, "Failed to resolve Fitbit authorization redirect for {Date}; falling back to API location.", targetDate);

                return Redirect(authEx.Location.ToString());
            }
        }
        catch (FitbitBackendUnavailableException unavailableEx)
        {
            _logger.LogWarning(unavailableEx, "Fitbit backend unavailable for {Date}", targetDate);
            var dateParameter = targetDate.ToString("yyyy-MM-dd");

            return View(FitnessDashboardViewModel.Empty(dateParameter, selectedUnit, unavailableEx.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load Fitbit dashboard data for {Date}", targetDate);
            var dateParameter = targetDate.ToString("yyyy-MM-dd");

            return View(FitnessDashboardViewModel.Empty(dateParameter, selectedUnit, "Unable to load Fitbit data at this time."));
        }
    }

    private static DateOnly ResolveDate(string date)
    {
        if (!string.IsNullOrWhiteSpace(date) && DateOnly.TryParse(date, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
        {
            return parsed;
        }

        return DateOnly.FromDateTime(DateTime.Today);
    }

    private static DaySummaryViewModel CreateSummary(FitbitDashboardModel model)
    {
        var summary = new DaySummaryViewModel
        {
            Date = ParseDate(model.Date),
            GeneratedUtc = model.GeneratedUtc,
            Weight = model.Weight ?? new FitbitWeightModel(),
            Calories = model.Calories ?? new FitbitCaloriesModel(),
            Sleep = model.Sleep ?? new FitbitSleepModel(),
            Activity = model.Activity ?? new FitbitActivityModel()
        };

        return HasAnyMetrics(summary)
                ? summary
                : null;
    }

    private static DateOnly ParseDate(string value)
    {
        if (DateOnly.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
        {
            return parsed;
        }

        return DateOnly.FromDateTime(DateTime.Today);
    }

    private static bool HasAnyMetrics(DaySummaryViewModel summary)
    {
        if (summary.Weight.StartingWeightKg.HasValue || summary.Weight.CurrentWeightKg.HasValue || summary.Weight.ChangeKg.HasValue || summary.Weight.BodyFatPercentage.HasValue || summary.Weight.LeanMassKg.HasValue)
        {
            return true;
        }

        if (summary.Calories.IntakeCalories.HasValue || summary.Calories.BurnedCalories.HasValue || summary.Calories.NetCalories.HasValue || summary.Calories.CarbsGrams.HasValue || summary.Calories.FatGrams.HasValue || summary.Calories.FiberGrams.HasValue || summary.Calories.ProteinGrams.HasValue || summary.Calories.SodiumMilligrams.HasValue)
        {
            return true;
        }

        if (summary.Sleep.TotalSleepHours.HasValue || summary.Sleep.TotalAwakeHours.HasValue || summary.Sleep.EfficiencyPercentage.HasValue || summary.Sleep.Levels.DeepMinutes.HasValue || summary.Sleep.Levels.LightMinutes.HasValue || summary.Sleep.Levels.RemMinutes.HasValue || summary.Sleep.Levels.WakeMinutes.HasValue)
        {
            return true;
        }

        if (summary.Activity.Steps.HasValue || summary.Activity.DistanceKm.HasValue || summary.Activity.Floors.HasValue)
        {
            return true;
        }

        return false;
    }

    private static FitnessRangeViewModel BuildRangeViewModel(string title, FitbitOverviewRangeModel range, WeightUnit unit)
    {
        if (range?.Items is null || range.Items.Count == 0)
        {
            return new FitnessRangeViewModel
            {
                Title = title,
                DateRangeLabel = string.Empty,
                Metrics = Array.Empty<FitnessMetricSeriesViewModel>(),
                HasData = false
            };
        }

        var labels = range.Items
                          .Select(item => item.Date)
                          .ToList();

        var weightSeries = range.Items
                                .Select(item => WeightUnitConverter.Convert(item.WeightKg, unit))
                                .ToList();
        var stepsSeries = range.Items
                               .Select(item => item.Steps.HasValue ? (double?)item.Steps.Value : null)
                               .ToList();
        var sleepSeries = range.Items
                               .Select(item => item.SleepHours)
                               .ToList();
        var netCaloriesSeries = range.Items
                                     .Select(item => item.NetCalories.HasValue ? (double?)item.NetCalories.Value : null)
                                     .ToList();

        var metrics = new List<FitnessMetricSeriesViewModel>
        {
            new()
            {
                Key = "weight",
                Title = "Weight",
                Unit = WeightUnitConverter.GetUnitLabel(unit),
                Labels = labels,
                Values = weightSeries
            },
            new()
            {
                Key = "steps",
                Title = "Steps",
                Unit = "steps",
                Labels = labels,
                Values = stepsSeries
            },
            new()
            {
                Key = "sleep",
                Title = "Sleep",
                Unit = "hrs",
                Labels = labels,
                Values = sleepSeries
            },
            new()
            {
                Key = "calories",
                Title = "Net calories",
                Unit = "kcal",
                Labels = labels,
                Values = netCaloriesSeries
            }
        };

        var hasData = metrics.Any(metric => metric.HasData);
        var dateRangeLabel = BuildDateRangeLabel(range.Items);

        return new FitnessRangeViewModel
        {
            Title = title,
            DateRangeLabel = dateRangeLabel,
            Metrics = metrics,
            HasData = hasData
        };
    }

    private static string BuildDateRangeLabel(IReadOnlyList<FitbitOverviewDayModel> items)
    {
        if (items is null || items.Count == 0)
        {
            return string.Empty;
        }

        var start = ParseDate(items[0].Date);
        var end = ParseDate(items[^1].Date);

        return $"{start:ddd dd MMM} - {end:ddd dd MMM}";
    }
}
