namespace Tomeshelf.Executor.Models;

/// <summary>
///     Payload supplied when manually executing an endpoint.
/// </summary>
public sealed class EndpointExecutionRequest
{
    /// <summary>
    ///     Optional override method. When omitted, the configured method is used.
    /// </summary>
    public string? Method { get; set; }

    /// <summary>
    ///     Optional override URL. When omitted, the configured endpoint URL is used.
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    ///     Optional request body sent to the upstream service.
    /// </summary>
    public string? Body { get; set; }

    /// <summary>
    ///     Additional headers applied on top of the configured defaults.
    /// </summary>
    public Dictionary<string, string>? Headers { get; set; }

    /// <summary>
    ///     Optional query string parameters that are appended to the request URL.
    /// </summary>
    public Dictionary<string, string>? Query { get; set; }

    /// <summary>
    ///     Optional timeout override (in seconds) for this execution.
    /// </summary>
    public int? TimeoutSeconds { get; set; }
}