using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Executor.Records;

namespace Tomeshelf.Executor.Services;

public interface IEndpointPingService
{
    Task<EndpointPingResult> SendAsync(Uri target, string method, Dictionary<string, string> headers, CancellationToken cancellationToken);
}