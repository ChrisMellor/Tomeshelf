namespace Tomeshelf.Executor.Models;

public sealed class ApiServiceOptionViewModel
{
    public required string ServiceName { get; init; }

    public required string DisplayName { get; init; }

    public required string BaseAddress { get; init; }
}