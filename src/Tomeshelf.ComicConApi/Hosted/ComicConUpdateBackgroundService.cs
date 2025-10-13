using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Tomeshelf.ComicConApi.Enums;
using Tomeshelf.ComicConApi.Services;
using Tomeshelf.Infrastructure.Queries;
using Tomeshelf.Infrastructure.Services;

namespace Tomeshelf.ComicConApi.Hosted;

/// <summary>
///     Background service that refreshes Comic Con guests on an hourly schedule.
///     Each run performs the upsert and cache refresh directly without additional
///     warmup delays. No immediate run occurs at startup.
/// </summary>
public sealed class ComicConUpdateBackgroundService : BackgroundService
{
    private readonly ILogger<ComicConUpdateBackgroundService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    /// <summary>
    ///     Initializes the scheduled background updater.
    /// </summary>
    /// <param name="scopeFactory">Factory to create DI scopes for resolving scoped services.</param>
    /// <param name="logger">Logger used for diagnostic output.</param>
    public ComicConUpdateBackgroundService(IServiceScopeFactory scopeFactory, ILogger<ComicConUpdateBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    /// <summary>
    ///     Runs the scheduling loop that triggers updates at the start of every hour
    ///     (server local time). Each scheduled run executes immediately once the delay
    ///     elapses.
    /// </summary>
    /// <param name="cancellationToken">Token that signals service shutdown.</param>
    /// <exception cref="OperationCanceledException">Thrown when the host is shutting down.</exception>
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("ComicCon update background service started");

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var now = DateTimeOffset.Now;
                var local = now.LocalDateTime;
                var nextHour = local.AddHours(1);
                var nextRun = new DateTimeOffset(nextHour);
                var delay = nextRun - now;

                _logger.LogInformation("Next ComicCon update scheduled at {NextRunLocal}", nextRun.LocalDateTime);

                try
                {
                    await Task.Delay(delay, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                try
                {
                    await RunOnceAsync(cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Scheduled ComicCon update failed at {RunTime}", DateTimeOffset.Now.LocalDateTime);
                }
            }
        }
        catch (OperationCanceledException) { }

        _logger.LogInformation("ComicCon update background service stopping");
    }

    /// <summary>
    ///     Performs a single update pass for all configured cities. Fetches latest
    ///     guests, upserts to the DB and refreshes the in-memory cache.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the update operation.</param>
    private async Task RunOnceAsync(CancellationToken cancellationToken)
    {
        foreach (var city in new[] { City.London, City.Birmingham })
            try
            {
                _logger.LogInformation("Updating guests for {CityName}", city);

                using var scope = _scopeFactory.CreateScope();
                var cityName = city.ToString();

                var guestService = scope.ServiceProvider.GetRequiredService<IGuestService>();
                await guestService.GetGuestsAsync(cityName, cancellationToken);

                var queries = scope.ServiceProvider.GetRequiredService<GuestQueries>();
                var groups = await queries.GetGuestsByCityAsync(cityName, cancellationToken);
                var total = groups.Sum(g => g.Items.Count);
                var cache = scope.ServiceProvider.GetRequiredService<IGuestsCache>();
                cache.Set(cityName, new GuestsSnapshot(cityName, total, groups, DateTimeOffset.UtcNow));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update guests for {CityName}", city);
            }
    }
}