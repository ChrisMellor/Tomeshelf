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

/// <summary>
///     Describes a discovered API base address.
/// </summary>
/// <param name="ServiceName">Logical resource name.</param>
/// <param name="DisplayName">Friendly label.</param>
/// <param name="BaseAddress">Absolute base URI.</param>
public sealed record ApiServiceDescriptor(string ServiceName, string DisplayName, string BaseAddress);