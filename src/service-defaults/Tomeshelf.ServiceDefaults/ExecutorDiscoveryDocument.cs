using System.Collections.Generic;

namespace Tomeshelf.ServiceDefaults;

/// <summary>
///     Response payload describing the endpoints available within a service.
/// </summary>
/// <param name="Service">Logical service name.</param>
/// <param name="Endpoints">Collection of discovered endpoints.</param>
public sealed record ExecutorDiscoveryDocument(string Service, IReadOnlyList<ExecutorDiscoveredEndpoint> Endpoints);