using System.Collections.Generic;

namespace Tomeshelf.ServiceDefaults;

/// <summary>
///     Response payload describing the endpoints available within a service.
/// </summary>
public sealed record ExecutorDiscoveryDocument
{
    /// <summary>
    ///     Response payload describing the endpoints available within a service.
    /// </summary>
    /// <param name="service">Logical service name.</param>
    /// <param name="endpoints">Collection of discovered endpoints.</param>
    public ExecutorDiscoveryDocument(string service, IReadOnlyList<ExecutorDiscoveredEndpoint> endpoints)
    {
        Service = service;
        Endpoints = endpoints;
    }

    /// <summary>Logical service name.</summary>
    public string Service { get; init; }

    /// <summary>Collection of discovered endpoints.</summary>
    public IReadOnlyList<ExecutorDiscoveredEndpoint> Endpoints { get; init; }

    public void Deconstruct(out string service, out IReadOnlyList<ExecutorDiscoveredEndpoint> endpoints)
    {
        service = Service;
        endpoints = Endpoints;
    }
}