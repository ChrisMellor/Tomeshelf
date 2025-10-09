using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Api.Enums;
using Tomeshelf.Api.Services;
using Tomeshelf.Infrastructure.Persistence;
using Tomeshelf.Infrastructure.Queries;
using Tomeshelf.Infrastructure.Services;

namespace Tomeshelf.Api.Hosted;

/// <summary>
/// Background service that refreshes Comic Con guests on a fixed schedule
/// (06:00, 12:00, 18:00 server local time). For each scheduled run it first
/// wakes the database (retrying until connectable), then waits 5 minutes to
/// allow full resume, and finally performs the upsert and cache refresh.
/// No immediate run occurs at startup.
/// </summary>
public sealed class ComicConUpdateBackgroundService : BackgroundService
{
    private readonly ILogger<ComicConUpdateBackgroundService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    /// <summary>
    /// Initializes the scheduled background updater.
    /// </summary>
    /// <param name="scopeFactory">Factory to create DI scopes for resolving scoped services.</param>
    /// <param name="logger">Logger.</param>
    public ComicConUpdateBackgroundService(IServiceScopeFactory scopeFactory, ILogger<ComicConUpdateBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    /// <summary>
    /// Runs the scheduling loop that triggers updates at 06:00, 12:00 and 18:00
    /// (server local time). Before each run, the service waits for the database
    /// to become connectable and then delays an additional 5 minutes to avoid
    /// failures during cold-start/resume.
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
                var nextRun = GetNextRunTime(now);
                var delay = nextRun - now;

                _logger.LogInformation("Next ComicCon update scheduled at {NextRunLocal}", nextRun.LocalDateTime);

                try { await Task.Delay(delay, cancellationToken); }
                catch (OperationCanceledException) { break; }

                try { await WaitForDatabaseAsync(cancellationToken); }
                catch (OperationCanceledException) { break; }

                try
                {
                    var resumeDelay = TimeSpan.FromMinutes(5);
                    _logger.LogInformation("DB ready. Waiting {Delay} minutes before upsert.", (int)resumeDelay.TotalMinutes);
                    await Task.Delay(resumeDelay, cancellationToken);
                }
                catch (OperationCanceledException) { break; }

                try { await RunOnceAsync(cancellationToken); }
                catch (OperationCanceledException) { break; }
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
    /// Performs a single update pass for all configured cities (after the
    /// scheduled wake-and-wait). Fetches latest guests, upserts to the DB and
    /// refreshes the in-memory cache.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the update operation.</param>
    private async Task RunOnceAsync(CancellationToken cancellationToken)
    {
        foreach (var city in new[] { City.London, City.Birmingham })
        {
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

    /// <summary>
    /// Attempts to connect to the database with exponential backoff until it is
    /// reachable. On serverless/auto-paused databases, the connection attempts
    /// act as a wake signal. Returns once <c>CanConnectAsync</c> is true or the
    /// operation is cancelled.
    /// </summary>
    private async Task WaitForDatabaseAsync(CancellationToken cancellationToken)
    {
        var delay = TimeSpan.FromSeconds(1);
        var maxDelay = TimeSpan.FromSeconds(15);
        var attempts = 0;

        while (!cancellationToken.IsCancellationRequested)
        {
            attempts++;
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<TomeshelfDbContext>();
                var isConnectable = await db.Database.CanConnectAsync(cancellationToken);
                if (isConnectable)
                {
                    _logger.LogInformation("Database reachable after {Attempts} attempt(s)", attempts);
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "Database not ready (attempt {Attempt}); retrying in {Delay}s", attempts, (int)delay.TotalSeconds);
            }

            try
            {
                await Task.Delay(delay, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            var nextMs = Math.Min(delay.TotalMilliseconds * 1.5, maxDelay.TotalMilliseconds);
            delay = TimeSpan.FromMilliseconds(nextMs);
        }
    }

    /// <summary>
    /// Computes the next scheduled run time (06:00, 12:00, 18:00) based on the
    /// provided timestamp, using server local time semantics.
    /// </summary>
    /// <param name="now">Current time (any offset).</param>
    /// <returns>The next scheduled time as a <see cref="DateTimeOffset"/>.</returns>
    private static DateTimeOffset GetNextRunTime(DateTimeOffset now)
    {
        var localNow = now.LocalDateTime;
        var today = localNow.Date;

        var at6 = today.AddHours(6);
        var at12 = today.AddHours(12);
        var at18 = today.AddHours(18);

        if (localNow <= at6)
        {
            return new DateTimeOffset(at6);
        }

        if (localNow <= at12)
        {
            return new DateTimeOffset(at12);
        }

        if (localNow <= at18)
        {
            return new DateTimeOffset(at18);
        }

        var tomorrow6 = today.AddDays(1).AddHours(6);
        return new DateTimeOffset(tomorrow6);
    }
}
