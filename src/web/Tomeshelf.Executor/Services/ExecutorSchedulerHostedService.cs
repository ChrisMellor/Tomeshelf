using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tomeshelf.Executor.Configuration;

namespace Tomeshelf.Executor.Services;

public sealed class ExecutorSchedulerHostedService : IHostedService
{
    private readonly ILogger<ExecutorSchedulerHostedService> _logger;
    private readonly IOptionsMonitor<ExecutorOptions> _optionsMonitor;
    private readonly IExecutorSchedulerOrchestrator _orchestrator;
    private IDisposable? _subscription;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ExecutorSchedulerHostedService" /> class.
    /// </summary>
    /// <param name="orchestrator">The orchestrator.</param>
    /// <param name="optionsMonitor">The options monitor.</param>
    /// <param name="logger">The logger.</param>
    public ExecutorSchedulerHostedService(IExecutorSchedulerOrchestrator orchestrator, IOptionsMonitor<ExecutorOptions> optionsMonitor, ILogger<ExecutorSchedulerHostedService> logger)
    {
        _orchestrator = orchestrator;
        _optionsMonitor = optionsMonitor;
        _logger = logger;
    }

    /// <summary>
    ///     Starts asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting Executor scheduler hosted service.");
        await _orchestrator.RefreshAsync(null, cancellationToken);

        _subscription = _optionsMonitor.OnChange((options, _) =>
        {
            Task.Run(async () => await _orchestrator.RefreshAsync(options), CancellationToken.None);
        });
    }

    /// <summary>
    ///     Stops asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping Executor scheduler hosted service.");
        _subscription?.Dispose();

        return Task.CompletedTask;
    }
}