using System.Net;
using Shouldly;
using Tomeshelf.SHiFT.Infrastructure.Services.External;

namespace Tomeshelf.SHiFT.Infrastructure.Tests.Services.External.ShiftWebSessionFactoryTests;

public class Create
{
    [Fact]
    public void UsesNamedHttpClient()
    {
        var httpClientFactory = new CapturingClientFactory();
        var factory = new ShiftWebSessionFactory(httpClientFactory);

        var session = factory.Create();

        session.ShouldBeOfType<ShiftWebSession>();
        httpClientFactory.LastName.ShouldBe(ShiftWebSession.HttpClientName);
    }

    private sealed class CapturingClientFactory : IHttpClientFactory
    {
        public string? LastName { get; private set; }

        public HttpClient CreateClient(string name)
        {
            LastName = name;

            return new HttpClient(new HttpMessageHandlerStub());
        }
    }

    private sealed class HttpMessageHandlerStub : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }
}