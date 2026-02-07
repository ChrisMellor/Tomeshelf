using System.Net;
using System.Text.Json;
using FakeItEasy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Shouldly;
using Tomeshelf.Executor.Services;
using Tomeshelf.Executor.Tests.TestUtilities;
using Tomeshelf.ServiceDefaults;

namespace Tomeshelf.Executor.Tests.Services.ApiEndpointDiscoveryServiceTests;

public class GetEndpointsAsync
{
    [Fact]
    public async Task WhenBaseUriInvalid_ReturnsEmpty()
    {
        var service = new ApiEndpointDiscoveryService(new ConfigurationBuilder().Build(), A.Fake<IHttpClientFactory>(), A.Fake<ILogger<ApiEndpointDiscoveryService>>());

        var results = await service.GetEndpointsAsync(" ", CancellationToken.None);

        results.ShouldBeEmpty();
    }

    [Fact]
    public async Task WhenRequestThrows_ReturnsEmpty()
    {
        var handler = new StubHttpMessageHandler((_, _) => throw new HttpRequestException("boom"));
        var client = new HttpClient(handler);
        var factory = new TestHttpClientFactory(client);
        var service = new ApiEndpointDiscoveryService(new ConfigurationBuilder().Build(), factory, A.Fake<ILogger<ApiEndpointDiscoveryService>>());

        var results = await service.GetEndpointsAsync("https://api.test", CancellationToken.None);

        results.ShouldBeEmpty();
    }

    [Fact]
    public async Task WhenResponseFails_ReturnsEmpty()
    {
        var handler = new StubHttpMessageHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadRequest)));
        var client = new HttpClient(handler);
        var factory = new TestHttpClientFactory(client);
        var service = new ApiEndpointDiscoveryService(new ConfigurationBuilder().Build(), factory, A.Fake<ILogger<ApiEndpointDiscoveryService>>());

        var results = await service.GetEndpointsAsync("https://api.test", CancellationToken.None);

        results.ShouldBeEmpty();
    }

    [Fact]
    public async Task WhenResponseOk_ReturnsEndpoints()
    {
        var endpoints = new List<ExecutorDiscoveredEndpoint> { new("id-1", "POST", "/path", "Display", "Desc", true, "Group") };
        var document = new ExecutorDiscoveryDocument("service", endpoints);
        var json = JsonSerializer.Serialize(document, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        var handler = new StubHttpMessageHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json) }));
        var client = new HttpClient(handler);
        var factory = new TestHttpClientFactory(client);
        var service = new ApiEndpointDiscoveryService(new ConfigurationBuilder().Build(), factory, A.Fake<ILogger<ApiEndpointDiscoveryService>>());

        var results = await service.GetEndpointsAsync("https://api.test", CancellationToken.None);

        var endpoint = results.ShouldHaveSingleItem();
        endpoint.Id.ShouldBe("id-1");
        var request = handler.Requests.ShouldHaveSingleItem();
        request.RequestUri.ShouldBe(new Uri("https://api.test/.well-known/executor-endpoints"));
    }
}