namespace Tomeshelf.Executor.Models;

public sealed record ErrorViewModel
{
    public ErrorViewModel(string requestId)
    {
        RequestId = requestId;
    }

    public bool ShowRequestId => !string.IsNullOrWhiteSpace(RequestId);

    public string RequestId { get; init; }

    public void Deconstruct(out string requestId)
    {
        requestId = RequestId;
    }
}