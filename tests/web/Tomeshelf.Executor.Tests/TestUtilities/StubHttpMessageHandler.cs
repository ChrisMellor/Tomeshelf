namespace Tomeshelf.Executor.Tests.TestUtilities;

public sealed class StubHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _handler;

    /// <summary>
    ///     Initializes a new instance of the <see cref="StubHttpMessageHandler" /> class.
    /// </summary>
    /// <param name="handler">The handler.</param>
    public StubHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler)
    {
        _handler = handler;
    }

    public List<HttpRequestMessage> Requests { get; } = new List<HttpRequestMessage>();

    /// <summary>
    ///     Sends asynchronously.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the operation result.</returns>
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Requests.Add(request);

        return await _handler(request, cancellationToken)
           .ConfigureAwait(false);
    }
}