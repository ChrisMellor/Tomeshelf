using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Executor.Configuration;

namespace Tomeshelf.Executor.Services;

public interface IExecutorConfigurationStore
{
    /// <summary>
    ///     Gets asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the operation result.</returns>
    Task<ExecutorOptions> GetAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Saves asynchronously.
    /// </summary>
    /// <param name="options">The options.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task SaveAsync(ExecutorOptions options, CancellationToken cancellationToken = default);
}