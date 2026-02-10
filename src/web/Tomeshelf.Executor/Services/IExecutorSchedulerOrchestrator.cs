using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Executor.Configuration;

namespace Tomeshelf.Executor.Services;

public interface IExecutorSchedulerOrchestrator
{
    /// <summary>
    ///     Refreshs asynchronously.
    /// </summary>
    /// <param name="options">The options.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task RefreshAsync(ExecutorOptions? options = null, CancellationToken cancellationToken = default);
}