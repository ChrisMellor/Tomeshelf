using System.Net;
using Shouldly;
using Tomeshelf.SHiFT.Infrastructure.Services.External;

namespace Tomeshelf.SHiFT.Infrastructure.Tests.Services.External.ShiftWebSessionFactoryTests;

public class Create
{
    /// <summary>
    ///     Uses the named http client.
    /// </summary>
    [Fact]
    public void UsesNamedHttpClient()
    {
        // Arrange
        var httpClientFactory = new CapturingClientFactory();
        var factory = new ShiftWebSessionFactory(httpClientFactory);

        // Act
        var session = factory.Create();

        // Assert
        session.ShouldBeOfType<ShiftWebSession>();
        httpClientFactory.LastName.ShouldBe(ShiftWebSession.HttpClientName);
    }

    private sealed class CapturingClientFactory : IHttpClientFactory
    {
        public string? LastName { get; private set; }

        /// <summary>
        ///     Creates the client.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The result of the operation.</returns>
        public HttpClient CreateClient(string name)
        {
            LastName = name;

            return new HttpClient(new HttpMessageHandlerStub());
        }
    }

    private sealed class HttpMessageHandlerStub : HttpMessageHandler
    {
        /// <summary>
        ///     Sends asynchronously.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the operation result.</returns>
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }
}