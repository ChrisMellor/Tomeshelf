using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Api.Enums;
using Tomeshelf.Infrastructure.Services;

namespace Tomeshelf.Api.Hosted;

/// <summary>
/// Background service that periodically refreshes Comic Con guests from the external API.
/// </summary>
public sealed class ComicConUpdateBackgroundService : BackgroundService
{
    private readonly ILogger<ComicConUpdateBackgroundService> _logger;
    private readonly IGuestService _guestService;

    /// <summary>
    /// Initializes a new instance of the background service.
    /// </summary>
    /// <param name="guestService">Service used to fetch and persist guests.</param>
    /// <param name="logger">Logger.</param>
    public ComicConUpdateBackgroundService(IGuestService guestService, ILogger<ComicConUpdateBackgroundService> logger)
    {
        _guestService = guestService;
        _logger = logger;
    }

    /// <summary>
    /// Runs the background loop that schedules hourly updates and invokes a single update pass.
    /// </summary>
    /// <param name="cancellationToken">Token that signals service shutdown.</param>
    /// <exception cref="OperationCanceledException">Thrown when the host is shutting down.</exception>
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("ComicCon update background service started");

        while (!cancellationToken.IsCancellationRequested)
        {
            var nowUtc = DateTimeOffset.UtcNow;
            var nextUtc = new DateTimeOffset(nowUtc.Year, nowUtc.Month, nowUtc.Day, nowUtc.Hour, 0, 0, TimeSpan.Zero)
                .AddHours(1);
            var delay = nextUtc - nowUtc;
            _logger.LogInformation("Next ComicCon update scheduled at {NextUtc}", nextUtc);

            try
            {
                await Task.Delay(delay, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            await RunOnceAsync(cancellationToken);
        }

        _logger.LogInformation("ComicCon update background service stopping");
    }

    /// <summary>
    /// Performs a single update pass for all configured cities.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the update operation.</param>
    private async Task RunOnceAsync(CancellationToken cancellationToken)
    {
        foreach (var city in new[] { City.London, City.Birmingham })
        {
            try
            {
                _logger.LogInformation("Updating guests for {CityName}", city);

                await _guestService.GetGuests(city.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update guests for {CityName}", city);
            }
        }
    }
}
