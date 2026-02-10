using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Tomeshelf.Executor.Services;

public interface IEndpointPingService
{
    /// <summary>
    ///     Sends asynchronously.
    /// </summary>
    /// <param name="target">The target.</param>
    /// <param name="method">The method.</param>
    /// <param name="headers">The headers.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the operation result.</returns>
    Task<EndpointPingResult> SendAsync(Uri target, string method, Dictionary<string, string>? headers, CancellationToken cancellationToken);
}