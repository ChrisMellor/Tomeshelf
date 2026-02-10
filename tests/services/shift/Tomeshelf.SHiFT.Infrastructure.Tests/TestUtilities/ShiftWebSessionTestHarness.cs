using System.Net;
using System.Text;
using Tomeshelf.SHiFT.Infrastructure.Services.External;

namespace Tomeshelf.SHiFT.Infrastructure.Tests.TestUtilities;

internal static class ShiftWebSessionTestHarness
{
    /// <summary>
    ///     Creates the session.
    /// </summary>
    /// <param name="handler">The handler.</param>
    /// <returns>The result of the operation.</returns>
    internal static ShiftWebSession CreateSession(RoutingHandler handler)
    {
        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://shift.test/")
        };

        var factory = new StubHttpClientFactory(client);

        return new ShiftWebSession(factory);
    }

    internal sealed class StubHttpClientFactory : IHttpClientFactory
    {
        private readonly HttpClient _client;

        /// <summary>
        ///     Initializes a new instance of the <see cref="StubHttpClientFactory" /> class.
        /// </summary>
        /// <param name="client">The client.</param>
        internal StubHttpClientFactory(HttpClient client)
        {
            _client = client;
        }

        /// <summary>
        ///     Creates the client.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The result of the operation.</returns>
        public HttpClient CreateClient(string name)
        {
            return _client;
        }
    }

    internal sealed class RoutingHandler : HttpMessageHandler
    {
        internal Dictionary<string, string> Responses { get; } = new Dictionary<string, string>();

        internal HttpRequestMessage? LastRequest { get; private set; }

        internal string? LastRequestBody { get; private set; }

        internal string? LastRequestContentType { get; private set; }

        /// <summary>
        ///     Sends asynchronously.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the operation result.</returns>
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            if (request.Content != null)
            {
                LastRequestContentType = request.Content.Headers.ContentType?.MediaType;
                LastRequestBody = request.Content.ReadAsStringAsync(cancellationToken)
                                         .GetAwaiter()
                                         .GetResult();
            }
            var path = request.RequestUri?.AbsolutePath ?? string.Empty;

            if (!Responses.TryGetValue(path, out var payload))
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
            }

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(payload, Encoding.UTF8, "text/html")
            };

            return Task.FromResult(response);
        }
    }
}
