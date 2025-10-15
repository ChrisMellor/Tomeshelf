#nullable enable
using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
    public async Task<IActionResult> Index([FromQuery] string? date, [FromQuery] bool refresh = false, CancellationToken cancellationToken = default)
    {
        var targetDate = ResolveDate(date);
        var today = DateOnly.FromDateTime(DateTime.Today);
        var shouldRefresh = refresh || (targetDate == today);

        using var scope = _logger.BeginScope(new
        {
                Date = targetDate,
                Refresh = shouldRefresh
        });
        _logger.LogInformation("Rendering Fitbit dashboard for {Date} (Refresh: {Refresh})", targetDate, shouldRefresh);

        try
        {
            var dateParameter = targetDate.ToString("yyyy-MM-dd");
            var returnUrl = Url.ActionLink("Index", "Fitness", new { date = dateParameter }) ?? $"{Request.Scheme}://{Request.Host}/fitness";

            var dashboard = await _fitbitApi.GetDashboardAsync(dateParameter, shouldRefresh, returnUrl, cancellationToken);

            if (dashboard is null)
            {
                return View(FitnessDashboardViewModel.Empty(dateParameter, "No Fitbit data is available for the selected date."));
            }

            var summary = CreateSummary(dashboard);
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
                    Summary = summary,
                    ErrorMessage = summary is null
                            ? "No Fitbit data is available for the selected date."
                            : null
            };

            return View(model);
        }
        catch (FitbitAuthorizationRequiredException authEx)
        {
            _logger.LogInformation("Redirecting to Fitbit authorization flow from dashboard request for {Date}", targetDate);

            return Redirect(authEx.Location.ToString());
        }
        catch (FitbitBackendUnavailableException unavailableEx)
        {
            _logger.LogWarning(unavailableEx, "Fitbit backend unavailable for {Date}", targetDate);
            var dateParameter = targetDate.ToString("yyyy-MM-dd");

            return View(FitnessDashboardViewModel.Empty(dateParameter, unavailableEx.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load Fitbit dashboard data for {Date}", targetDate);
            var dateParameter = targetDate.ToString("yyyy-MM-dd");

            return View(FitnessDashboardViewModel.Empty(dateParameter, "Unable to load Fitbit data at this time."));
        }
    }

    private static DateOnly ResolveDate(string? date)
    {
        if (!string.IsNullOrWhiteSpace(date) && DateOnly.TryParse(date, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
        {
            return parsed;
        }

        return DateOnly.FromDateTime(DateTime.Today);
    }

    private static DaySummaryViewModel? CreateSummary(FitbitDashboardModel model)
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
        if (summary.Weight.StartingWeightKg.HasValue || summary.Weight.CurrentWeightKg.HasValue || summary.Weight.ChangeKg.HasValue)
        {
            return true;
        }

        if (summary.Calories.IntakeCalories.HasValue || summary.Calories.BurnedCalories.HasValue || summary.Calories.NetCalories.HasValue)
        {
            return true;
        }

        if (summary.Sleep.TotalSleepHours.HasValue || summary.Sleep.TotalAwakeHours.HasValue || summary.Sleep.EfficiencyPercentage.HasValue)
        {
            return true;
        }

        if (summary.Activity.Steps.HasValue || summary.Activity.DistanceKm.HasValue || summary.Activity.Floors.HasValue)
        {
            return true;
        }

        return false;
    }
}