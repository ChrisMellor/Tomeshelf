using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Application.Contracts.Fitbit;
using Tomeshelf.Infrastructure.Fitness;

namespace Tomeshelf.Fitbit.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class FitbitController : ControllerBase
{
    private readonly FitbitDashboardService _dashboardService;
    private readonly ILogger<FitbitController> _logger;

    public FitbitController(FitbitDashboardService dashboardService, ILogger<FitbitController> logger)
    {
        _dashboardService = dashboardService;
        _logger = logger;
    }

    /// <summary>
    ///     Returns a daily Fitbit snapshot for the supplied date (defaults to today).
    /// </summary>
    /// <param name="date">Optional ISO date (yyyy-MM-dd). When omitted, the current local date is used.</param>
    /// <param name="refresh">When true, forces the snapshot to be refreshed from Fitbit even for past dates.</param>
    /// <param name="returnUrl">Return URL used when redirecting to the Fitbit authorization flow.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    [HttpGet("Dashboard")]
    [ProducesResponseType(typeof(FitbitDashboardDto), 200)]
    public async Task<ActionResult<FitbitDashboardDto>> GetDashboard([FromQuery] string date, [FromQuery] bool refresh = false, [FromQuery] string returnUrl = null, CancellationToken cancellationToken = default)
    {
        var targetDate = ResolveDate(date);
        var today = DateOnly.FromDateTime(DateTime.Today);
        var shouldRefresh = refresh || (targetDate == today);

        using var scope = _logger.BeginScope(new
        {
            Date = targetDate,
            Refresh = shouldRefresh
        });
        _logger.LogInformation("Fetching Fitbit snapshot for {Date} (Refresh: {Refresh})", targetDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), shouldRefresh);

        try
        {
            var snapshot = await _dashboardService.GetDashboardAsync(targetDate, shouldRefresh, cancellationToken);
            if (snapshot is null)
            {
                return NotFound();
            }

            return Ok(snapshot);
        }
        catch (HttpRequestException ex) when ((ex.StatusCode == HttpStatusCode.Unauthorized) || (ex.StatusCode == HttpStatusCode.Forbidden))
        {
            _logger.LogWarning(ex, "Fitbit API returned unauthorized for {Date}. Redirecting to authorization flow.", targetDate);
            var authorizeEndpoint = BuildAuthorizeRedirectTarget(returnUrl);

            return Redirect(authorizeEndpoint);
        }
        catch (FitbitRateLimitExceededException ex)
        {
            _logger.LogWarning(ex, "Fitbit API rate limit reached while fetching data for {Date}.", targetDate);

            var retryAfterSeconds = ex.RetryAfter.HasValue
                ? (int)Math.Ceiling(Math.Max(0, ex.RetryAfter.Value.TotalSeconds))
                : (int?)null;

            return retryAfterSeconds.HasValue
                ? StatusCode(429, new
                {
                    message = ex.Message,
                    retryAfterSeconds
                })
                : StatusCode(429, new
                {
                    message = ex.Message
                });
        }
        catch (FitbitBadRequestException ex)
        {
            _logger.LogWarning(ex, "Fitbit API returned a bad request for {Date}.", targetDate);

            return StatusCode(502, new
            {
                message = ex.Message
            });
        }
        catch (HttpRequestException ex) when ((ex.StatusCode == HttpStatusCode.ServiceUnavailable) || (ex.StatusCode == HttpStatusCode.GatewayTimeout) || (ex.StatusCode == HttpStatusCode.BadGateway))
        {
            _logger.LogWarning(ex, "Fitbit API is unavailable while fetching data for {Date}.", targetDate);

            return StatusCode(503, new
            {
                message = "Fitbit service is unavailable right now. Please try again shortly."
            });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Unexpected Fitbit API failure for {Date}", targetDate);

            return StatusCode(502, new
            {
                message = "Unable to retrieve Fitbit data due to an unexpected error."
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogInformation(ex, "Fitbit dashboard request could not be completed due to missing configuration. Redirecting to authorization flow.");

            var authorizeEndpoint = BuildAuthorizeRedirectTarget(returnUrl);

            return Redirect(authorizeEndpoint);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning(ex, "Fitbit dashboard request timed out for {Date}.", targetDate);

            return StatusCode(503, new
            {
                message = "Fitbit request timed out. Please try again in a moment."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Fitbit dashboard data for {Date}", targetDate);

            return StatusCode(500, new
            {
                message = "Failed to retrieve Fitbit data."
            });
        }
    }

    private string BuildAuthorizeRedirectTarget(string requestedReturnUrl)
    {
        var queryReturnUrl = requestedReturnUrl ?? Request.Query["returnUrl"];
        var target = string.IsNullOrWhiteSpace(queryReturnUrl)
            ? "/fitness"
            : queryReturnUrl!;

        var authorizeEndpoint = Url.ActionLink("Authorize", "FitbitAuthorization", new
        {
            returnUrl = target
        });
        if (!string.IsNullOrWhiteSpace(authorizeEndpoint))
        {
            return authorizeEndpoint!;
        }

        return $"/api/fitbit/auth/authorize?returnUrl={Uri.EscapeDataString(target)}";
    }

    private static DateOnly ResolveDate(string input)
    {
        if (!string.IsNullOrWhiteSpace(input) && DateOnly.TryParseExact(input, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
        {
            return parsed;
        }

        return DateOnly.FromDateTime(DateTime.Now);
    }
}