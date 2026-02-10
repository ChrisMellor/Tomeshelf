using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Tomeshelf.Web.Models;
using Tomeshelf.Web.Models.Home;
using Tomeshelf.Web.Services;

namespace Tomeshelf.Web.Controllers;

/// <summary>
///     MVC controller for site home and informational pages.
/// </summary>
public class HomeController : Controller
{
    private readonly IBundlesApi _bundlesApi;
    private readonly IFitbitApi _fitbitApi;
    private readonly IGuestsApi _guestsApi;
    private readonly ILogger<HomeController> _logger;
    private readonly IPaissaApi _paissaApi;

    /// <summary>
    ///     Initializes a new instance of the <see cref="HomeController" /> class.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    public HomeController(IBundlesApi bundlesApi, IFitbitApi fitbitApi, IGuestsApi guestsApi, IPaissaApi paissaApi, ILogger<HomeController> logger)
    {
        _bundlesApi = bundlesApi;
        _fitbitApi = fitbitApi;
        _guestsApi = guestsApi;
        _paissaApi = paissaApi;
        _logger = logger;
    }

    /// <summary>
    ///     Renders the error page with current request identifier.
    /// </summary>
    /// <returns>The error view.</returns>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        var errorViewModel = new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier };

        return View(errorViewModel);
    }

    /// <summary>
    ///     Renders the home page.
    /// </summary>
    /// <returns>The home view.</returns>
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var eventsTask = GetEventsSummaryAsync(cancellationToken);
        var bundlesTask = GetBundlesSummaryAsync(cancellationToken);
        var fitnessTask = GetFitnessSummaryAsync(cancellationToken);
        var gamingTask = GetGamingSummaryAsync(cancellationToken);

        var model = new HomeIndexViewModel
        {
            EventsSummary = await eventsTask,
            EducationSummary = await bundlesTask,
            HealthSummary = await fitnessTask,
            GamingSummary = await gamingTask
        };

        return View(model);
    }

    /// <summary>
    ///     Renders the privacy policy page.
    /// </summary>
    /// <returns>The privacy view.</returns>
    public IActionResult Privacy()
    {
        return View();
    }

    /// <summary>
    ///     Gets the bundles summary asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the operation result.</returns>
    private async Task<string> GetBundlesSummaryAsync(CancellationToken cancellationToken)
    {
        try
        {
            var bundles = await _bundlesApi.GetBundlesAsync(false, cancellationToken);
            var count = bundles?.Count ?? 0;

            return count == 1
                ? "1 bundle live"
                : $"{count.ToString("N0", CultureInfo.CurrentCulture)} bundles live";
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unable to load bundle summary for home.");

            return "Bundles unavailable";
        }
    }

    /// <summary>
    ///     Gets the events summary asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the operation result.</returns>
    private async Task<string> GetEventsSummaryAsync(CancellationToken cancellationToken)
    {
        try
        {
            var events = await _guestsApi.GetComicConEventsAsync(cancellationToken);
            var count = events?.Count ?? 0;

            return count == 1
                ? "1 event configured"
                : $"{count.ToString("N0", CultureInfo.CurrentCulture)} events configured";
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unable to load Comic Con event summary for home.");

            return "Events unavailable";
        }
    }

    /// <summary>
    ///     Gets the fitness summary asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the operation result.</returns>
    private async Task<string> GetFitnessSummaryAsync(CancellationToken cancellationToken)
    {
        try
        {
            var today = DateOnly.FromDateTime(DateTime.Today)
                                .ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            var returnUrl = Url.ActionLink("Index", "Home") ?? $"{Request.Scheme}://{Request.Host}/";

            var dashboard = await _fitbitApi.GetDashboardAsync(today, false, returnUrl, cancellationToken);
            if (dashboard?.Activity?.Steps is int steps)
            {
                return $"{steps.ToString("N0", CultureInfo.CurrentCulture)} steps today";
            }

            if (dashboard?.Calories?.BurnedCalories is int burned)
            {
                return $"{burned.ToString("N0", CultureInfo.CurrentCulture)} kcal burned";
            }

            if (dashboard?.Sleep?.TotalSleepHours is double hours)
            {
                return $"{hours.ToString("0.#", CultureInfo.CurrentCulture)}h sleep";
            }

            return "Fitbit data available";
        }
        catch (FitbitAuthorizationRequiredException)
        {
            return "Connect Fitbit to sync";
        }
        catch (FitbitBackendUnavailableException)
        {
            return "Fitbit temporarily unavailable";
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unable to load Fitbit summary for home.");

            return "Fitbit unavailable";
        }
    }

    /// <summary>
    ///     Gets the gaming summary asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the operation result.</returns>
    private async Task<string> GetGamingSummaryAsync(CancellationToken cancellationToken)
    {
        try
        {
            var world = await _paissaApi.GetWorldAsync(cancellationToken);
            var totalPlots = 0;

            if (world?.Districts is not null)
            {
                foreach (var district in world.Districts)
                {
                    foreach (var tab in district.Tabs)
                    {
                        totalPlots += tab.Plots.Count;
                    }
                }
            }

            return totalPlots == 1
                ? "1 plot listed"
                : $"{totalPlots.ToString("N0", CultureInfo.CurrentCulture)} plots listed";
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unable to load Paissa summary for home.");

            return "Gaming unavailable";
        }
    }
}