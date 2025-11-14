namespace Tomeshelf.Executor.Models;

public sealed record ErrorViewModel(string? RequestId)
{
    public bool ShowRequestId => !string.IsNullOrWhiteSpace(RequestId);
}
