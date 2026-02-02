using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Tomeshelf.Executor.Services;
using Tomeshelf.Executor.Tests.TestUtilities;

namespace Tomeshelf.Executor.Tests.Services;

public class EndpointPingServiceTests
{
    [Fact]
    public async Task SendAsync_ReturnsSuccessForOkResponses()
    {
        var handler = new StubHttpMessageHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            ReasonPhrase = "OK",
            Content = new StringContent("pong")
        }));
        var client = new HttpClient(handler);
        var factory = new TestHttpClientFactory(client);
        var service = new EndpointPingService(factory, A.Fake<ILogger<EndpointPingService>>());

        var headers = new Dictionary<string, string>
        {
            ["X-Test"] = "value",
            ["Content-Type"] = "application/json"
        };

        var result = await service.SendAsync(new Uri("https://example.test"), "PUT", headers, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Body.Should().Be("pong");
        result.Message.Should().Be("OK");

        var request = handler.Requests.Should().ContainSingle().Subject;
        request.Method.Should().Be(HttpMethod.Put);
        request.Headers.Contains("X-Test").Should().BeTrue();

        IEnumerable<string>? values = null;
        if (request.Headers.TryGetValues("Content-Type", out var requestValues))
        {
            values = requestValues;
        }
        else if (request.Content?.Headers.TryGetValues("Content-Type", out var contentValues) == true)
        {
            values = contentValues;
        }

        values.Should().NotBeNull();
        values!.Should().Contain("application/json");
    }

    [Fact]
    public async Task SendAsync_WhenMethodMissing_UsesPost()
    {
        var handler = new StubHttpMessageHandler((request, _) =>
        {
            request.Method.Should().Be(HttpMethod.Post);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.Accepted));
        });
        var client = new HttpClient(handler);
        var factory = new TestHttpClientFactory(client);
        var service = new EndpointPingService(factory, A.Fake<ILogger<EndpointPingService>>());

        var result = await service.SendAsync(new Uri("https://example.test"), " ", null, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.StatusCode.Should().Be(202);
    }

    [Fact]
    public async Task SendAsync_WhenRequestFails_ReturnsFailure()
    {
        var handler = new StubHttpMessageHandler((_, _) => throw new HttpRequestException("boom"));
        var client = new HttpClient(handler);
        var factory = new TestHttpClientFactory(client);
        var service = new EndpointPingService(factory, A.Fake<ILogger<EndpointPingService>>());

        var result = await service.SendAsync(new Uri("https://example.test"), "POST", null, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.StatusCode.Should().BeNull();
        result.Message.Should().Be("boom");
        result.Body.Should().BeNull();
    }
}
