using Tomeshelf.Executor.Configuration;

namespace Tomeshelf.Executor.Services;

public interface IExecutorSchedulerOrchestrator
{
    Task RefreshAsync(ExecutorOptions? options = null, CancellationToken cancellationToken = default);
}
