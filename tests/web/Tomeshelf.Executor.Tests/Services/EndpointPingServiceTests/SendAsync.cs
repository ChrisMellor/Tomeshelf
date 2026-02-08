using System.Net;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using Shouldly;
using Tomeshelf.Executor.Services;
using Tomeshelf.Executor.Tests.TestUtilities;

namespace Tomeshelf.Executor.Tests.Services.EndpointPingServiceTests;

public class SendAsync
{
    [Fact]
    public async Task ReturnsSuccessForOkResponses()
    {
        // Arrange
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

        // Act
        var result = await service.SendAsync(new Uri("https://example.test"), "PUT", headers, CancellationToken.None);

        // Assert
        result.Success.ShouldBeTrue();
        result.StatusCode.ShouldBe(200);
        result.Body.ShouldBe("pong");
        result.Message.ShouldBe("OK");

        var request = handler.Requests.ShouldHaveSingleItem();
        request.Method.ShouldBe(HttpMethod.Put);
        request.Headers
               .Contains("X-Test")
               .ShouldBeTrue();

        IEnumerable<string>? values = null;
        if (request.Headers.TryGetValues("Content-Type", out var requestValues))
        {
            values = requestValues;
        }
        else if (request.Content?.Headers.TryGetValues("Content-Type", out var contentValues) == true)
        {
            values = contentValues;
        }

        values.ShouldNotBeNull();
        values!.ShouldContain("application/json");
    }

    [Fact]
    public async Task WhenMethodMissing_UsesPost()
    {
        // Arrange
        // Act
        // Assert
        var handler = new StubHttpMessageHandler((request, _) =>
        {
            request.Method.ShouldBe(HttpMethod.Post);

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.Accepted));
        });
        var client = new HttpClient(handler);
        var factory = new TestHttpClientFactory(client);
        var service = new EndpointPingService(factory, A.Fake<ILogger<EndpointPingService>>());

        var result = await service.SendAsync(new Uri("https://example.test"), " ", null, CancellationToken.None);

        result.Success.ShouldBeTrue();
        result.StatusCode.ShouldBe(202);
    }

    [Fact]
    public async Task WhenRequestFails_ReturnsFailure()
    {
        // Arrange
        var handler = new StubHttpMessageHandler((_, _) => throw new HttpRequestException("boom"));
        var client = new HttpClient(handler);
        var factory = new TestHttpClientFactory(client);
        var service = new EndpointPingService(factory, A.Fake<ILogger<EndpointPingService>>());

        // Act
        var result = await service.SendAsync(new Uri("https://example.test"), "POST", null, CancellationToken.None);

        // Assert
        result.Success.ShouldBeFalse();
        result.StatusCode.ShouldBeNull();
        result.Message.ShouldBe("boom");
        result.Body.ShouldBeNull();
    }
}