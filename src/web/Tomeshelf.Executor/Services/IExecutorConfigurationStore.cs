using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Executor.Configuration;

namespace Tomeshelf.Executor.Services;

public interface IExecutorConfigurationStore
{
    Task<ExecutorOptions> GetAsync(CancellationToken cancellationToken = default);

    Task SaveAsync(ExecutorOptions options, CancellationToken cancellationToken = default);
}