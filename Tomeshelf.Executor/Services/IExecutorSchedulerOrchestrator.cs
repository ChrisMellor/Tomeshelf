namespace Tomeshelf.Executor.Services;

public interface IExecutorSchedulerOrchestrator
{
    Task RefreshAsync(CancellationToken cancellationToken = default);
}
