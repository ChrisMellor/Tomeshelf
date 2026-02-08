using FakeItEasy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Shouldly;
using Tomeshelf.Executor.Services;

namespace Tomeshelf.Executor.Tests.Services.ApiEndpointDiscoveryServiceTests;

public class GetApisAsync
{
    [Fact]
    public async Task ReturnsDistinctOrderedServices()
    {
        // Arrange
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

        // Act
        var results = await service.GetApisAsync(CancellationToken.None);

        // Assert
        results.Count.ShouldBe(3);
        results[0]
           .ServiceName
           .ShouldBe("alpha");
        results[0]
           .BaseAddress
           .ShouldBe("https://alpha.test");
        results[0]
           .DisplayName
           .ShouldBe("alpha (HTTP)");
        results[1]
           .ServiceName
           .ShouldBe("beta");
        results[1]
           .BaseAddress
           .ShouldBe("https://beta.test");
        results[2]
           .ServiceName
           .ShouldBe("gamma");
    }

    [Fact]
    public async Task WhenNoServicesSection_ReturnsEmpty()
    {
        // Arrange
        var config = new ConfigurationBuilder().Build();
        var service = new ApiEndpointDiscoveryService(config, A.Fake<IHttpClientFactory>(), A.Fake<ILogger<ApiEndpointDiscoveryService>>());

        // Act
        var results = await service.GetApisAsync(CancellationToken.None);

        // Assert
        results.ShouldBeEmpty();
    }
}