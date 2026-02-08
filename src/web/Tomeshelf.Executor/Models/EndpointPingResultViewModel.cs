using System;

namespace Tomeshelf.Executor.Models;

public sealed class EndpointPingResultViewModel
{
    public bool Success { get; init; }

    public string Message { get; init; } = string.Empty;

    public int? StatusCode { get; init; }

    public string? ResponseBody { get; init; }

    public TimeSpan Duration { get; init; }
}