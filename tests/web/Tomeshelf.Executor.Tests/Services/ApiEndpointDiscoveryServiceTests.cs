using System.Net;
using System.Text.Json;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Tomeshelf.Executor.Services;
using Tomeshelf.Executor.Tests.TestUtilities;
using Tomeshelf.ServiceDefaults;

namespace Tomeshelf.Executor.Tests.Services;

public class ApiEndpointDiscoveryServiceTests
{
    [Fact]
    public async Task GetApisAsync_ReturnsDistinctOrderedServices()
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
                                                {
                                                    ["services:alpha:http:0"] = "https://alpha.test/",
                                                    ["services:alpha:https:0"] = "https://alpha.test/",
                                                    ["services:beta:https:0"] = "https://beta.test",
                                                    ["services:beta:http:0"] = "not-a-uri",
                                                    ["services:gamma:http:0"] = "https://gamma.test/"
                                                })
                                               .Build();

        var service = new ApiEndpointDiscoveryService(config, A.Fake<IHttpClientFactory>(), A.Fake<ILogger<ApiEndpointDiscoveryService>>());

        var results = await service.GetApisAsync(CancellationToken.None);

        results.Should()
               .HaveCount(3);
        results[0]
           .ServiceName
           .Should()
           .Be("alpha");
        results[0]
           .BaseAddress
           .Should()
           .Be("https://alpha.test");
        results[0]
           .DisplayName
           .Should()
           .Be("alpha (HTTP)");
        results[1]
           .ServiceName
           .Should()
           .Be("beta");
        results[1]
           .BaseAddress
           .Should()
           .Be("https://beta.test");
        results[2]
           .ServiceName
           .Should()
           .Be("gamma");
    }

    [Fact]
    public async Task GetApisAsync_WhenNoServicesSection_ReturnsEmpty()
    {
        var config = new ConfigurationBuilder().Build();
        var service = new ApiEndpointDiscoveryService(config, A.Fake<IHttpClientFactory>(), A.Fake<ILogger<ApiEndpointDiscoveryService>>());

        var results = await service.GetApisAsync(CancellationToken.None);

        results.Should()
               .BeEmpty();
    }

    [Fact]
    public async Task GetEndpointsAsync_WhenBaseUriInvalid_ReturnsEmpty()
    {
        var service = new ApiEndpointDiscoveryService(new ConfigurationBuilder().Build(), A.Fake<IHttpClientFactory>(), A.Fake<ILogger<ApiEndpointDiscoveryService>>());

        var results = await service.GetEndpointsAsync(" ", CancellationToken.None);

        results.Should()
               .BeEmpty();
    }

    [Fact]
    public async Task GetEndpointsAsync_WhenRequestThrows_ReturnsEmpty()
    {
        var handler = new StubHttpMessageHandler((_, _) => throw new HttpRequestException("boom"));
        var client = new HttpClient(handler);
        var factory = new TestHttpClientFactory(client);
        var service = new ApiEndpointDiscoveryService(new ConfigurationBuilder().Build(), factory, A.Fake<ILogger<ApiEndpointDiscoveryService>>());

        var results = await service.GetEndpointsAsync("https://api.test", CancellationToken.None);

        results.Should()
               .BeEmpty();
    }

    [Fact]
    public async Task GetEndpointsAsync_WhenResponseFails_ReturnsEmpty()
    {
        var handler = new StubHttpMessageHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadRequest)));
        var client = new HttpClient(handler);
        var factory = new TestHttpClientFactory(client);
        var service = new ApiEndpointDiscoveryService(new ConfigurationBuilder().Build(), factory, A.Fake<ILogger<ApiEndpointDiscoveryService>>());

        var results = await service.GetEndpointsAsync("https://api.test", CancellationToken.None);

        results.Should()
               .BeEmpty();
    }

    [Fact]
    public async Task GetEndpointsAsync_WhenResponseOk_ReturnsEndpoints()
    {
        var endpoints = new List<ExecutorDiscoveredEndpoint> { new ExecutorDiscoveredEndpoint("id-1", "POST", "/path", "Display", "Desc", true, "Group") };
        var document = new ExecutorDiscoveryDocument("service", endpoints);
        var json = JsonSerializer.Serialize(document, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        var handler = new StubHttpMessageHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json) }));
        var client = new HttpClient(handler);
        var factory = new TestHttpClientFactory(client);
        var service = new ApiEndpointDiscoveryService(new ConfigurationBuilder().Build(), factory, A.Fake<ILogger<ApiEndpointDiscoveryService>>());

        var results = await service.GetEndpointsAsync("https://api.test", CancellationToken.None);

        results.Should()
               .ContainSingle();
        results[0]
           .Id
           .Should()
           .Be("id-1");
        handler.Requests
               .Single()
               .RequestUri
               .Should()
               .Be(new Uri("https://api.test/.well-known/executor-endpoints"));
    }
}