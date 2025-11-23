namespace Tomeshelf.Executor.Models;

public sealed class EndpointSummaryViewModel
{
    public required string Name { get; init; }

    public required string Url { get; init; }

    public required string Method { get; init; }

    public required string Cron { get; init; }

    public bool Enabled { get; init; }

    public string HeadersDisplay { get; init; } = string.Empty;
}