using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Api.Enums;
using Tomeshelf.Api.Services;
using Tomeshelf.Infrastructure.Queries;

namespace Tomeshelf.Api.Hosted;

/// <summary>
/// Performs a one-time, non-blocking cache warmup shortly after startup.
/// Attempts to build snapshots for known cities with a short timeout, but
/// does not block the host if the database is resuming.
/// </summary>
public sealed class CacheWarmupHostedService : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CacheWarmupHostedService> _logger;

    public CacheWarmupHostedService(IServiceScopeFactory scopeFactory, ILogger<CacheWarmupHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _ = Task.Run(() => WarmAsync(cancellationToken), CancellationToken.None);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private async Task WarmAsync(CancellationToken stoppingToken)
    {
        try
        {
            foreach (var city in new[] { City.London, City.Birmingham })
            {
                if (stoppingToken.IsCancellationRequested) break;
                await WarmCityAsync(city.ToString(), stoppingToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Cache warmup encountered an error");
        }
    }

    private async Task WarmCityAsync(string city, CancellationToken stoppingToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var queries = scope.ServiceProvider.GetRequiredService<GuestQueries>();
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
            cts.CancelAfter(TimeSpan.FromSeconds(3));
            var groups = await queries.GetGuestsByCityAsync(city, cts.Token);
            var total = groups.Sum(g => g.Items.Count);

            var cache = scope.ServiceProvider.GetRequiredService<IGuestsCache>();
            cache.Set(city, new GuestsSnapshot(city, total, groups, DateTimeOffset.UtcNow));
            _logger.LogInformation("Cache warmup stored snapshot for {City}: {Total} guests", city, total);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Cache warmup timed out for {City}; will rely on first request or background update", city);
        }
        catch (Exception ex)
        {
            _logger.LogInformation(ex, "Cache warmup failed for {City}", city);
        }
    }
}

