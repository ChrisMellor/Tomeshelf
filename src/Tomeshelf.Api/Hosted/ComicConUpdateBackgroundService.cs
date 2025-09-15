using Microsoft.Extensions.DependencyInjection;
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
    private readonly IServiceScopeFactory _scopeFactory;

    /// <summary>
    /// Initializes a new instance of the background service.
    /// </summary>
    /// <param name="scopeFactory">Factory to create DI scopes for resolving scoped services.</param>
    /// <param name="logger">Logger.</param>
    public ComicConUpdateBackgroundService(IServiceScopeFactory scopeFactory, ILogger<ComicConUpdateBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
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

        var timer = new PeriodicTimer(TimeSpan.FromHours(1));
        try
        {
            while (await timer.WaitForNextTickAsync(cancellationToken))
            {
                await RunOnceAsync(cancellationToken);
            }
        }
        catch (OperationCanceledException) { }

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

                using var scope = _scopeFactory.CreateScope();
                var guestService = scope.ServiceProvider.GetRequiredService<IGuestService>();
                await guestService.GetGuestsAsync(city.ToString(), cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update guests for {CityName}", city);
            }
        }
    }
}
