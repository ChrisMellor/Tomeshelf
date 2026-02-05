namespace Tomeshelf.Executor.Tests.TestUtilities;

public sealed class StubHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _handler;

    public StubHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler)
    {
        _handler = handler;
    }

    public List<HttpRequestMessage> Requests { get; } = new List<HttpRequestMessage>();

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Requests.Add(request);

        return await _handler(request, cancellationToken)
           .ConfigureAwait(false);
    }
}