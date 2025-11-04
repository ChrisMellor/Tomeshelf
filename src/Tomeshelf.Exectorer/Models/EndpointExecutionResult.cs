namespace Tomeshelf.Executor.Models;

/// <summary>
///     Represents the outcome of a manual or scheduled execution.
/// </summary>
public sealed class EndpointExecutionResult
{
    public string EndpointId { get; init; } = string.Empty;

    public string DisplayName { get; init; } = string.Empty;

    public string Method { get; init; } = string.Empty;

    public string Target { get; init; } = string.Empty;

    public bool Success { get; init; }

    public int StatusCode { get; init; }

    public string ReasonPhrase { get; init; } = string.Empty;

    public string? ContentType { get; init; }

    public string Body { get; init; } = string.Empty;

    public bool BodyTruncated { get; init; }

    public IDictionary<string, string[]> Headers { get; init; } = new Dictionary<string, string[]>();

    public double DurationMilliseconds { get; init; }

    public DateTimeOffset ExecutedAt { get; init; }
}