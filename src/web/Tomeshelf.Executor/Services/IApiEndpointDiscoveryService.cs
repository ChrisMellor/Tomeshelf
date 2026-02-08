using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.ServiceDefaults;

namespace Tomeshelf.Executor.Services;

/// <summary>
///     Provides discovery information about downstream APIs and their executor endpoints.
/// </summary>
public interface IApiEndpointDiscoveryService
{
    /// <summary>
    ///     Returns the list of API base addresses available to the executor.
    /// </summary>
    Task<IReadOnlyList<ApiServiceDescriptor>> GetApisAsync(CancellationToken cancellationToken);

    /// <summary>
    ///     Returns the executor-compatible endpoints exposed by a specific API base address.
    /// </summary>
    Task<IReadOnlyList<ExecutorDiscoveredEndpoint>> GetEndpointsAsync(string baseAddress, CancellationToken cancellationToken);
}