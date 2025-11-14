using Microsoft.Extensions.Options;
using Tomeshelf.Executor.Configuration;

namespace Tomeshelf.Executor.Services;

public sealed class ExecutorSchedulerHostedService : IHostedService
{
    private readonly ILogger<ExecutorSchedulerHostedService> _logger;
    private readonly IOptionsMonitor<ExecutorOptions> _optionsMonitor;
    private readonly IExecutorSchedulerOrchestrator _orchestrator;
    private IDisposable? _subscription;

    public ExecutorSchedulerHostedService(IExecutorSchedulerOrchestrator orchestrator, IOptionsMonitor<ExecutorOptions> optionsMonitor, ILogger<ExecutorSchedulerHostedService> logger)
    {
        _orchestrator = orchestrator;
        _optionsMonitor = optionsMonitor;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting Executor scheduler hosted service.");
        await _orchestrator.RefreshAsync(cancellationToken);

        _subscription = _optionsMonitor.OnChange((_, _) =>
        {
            _ = Task.Run(() => _orchestrator.RefreshAsync(), CancellationToken.None);
        });
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping Executor scheduler hosted service.");
        _subscription?.Dispose();

        return Task.CompletedTask;
    }
}