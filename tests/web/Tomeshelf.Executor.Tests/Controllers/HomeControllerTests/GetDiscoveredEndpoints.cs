using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using Shouldly;
using Tomeshelf.Executor.Configuration;
using Tomeshelf.Executor.Services;
using Tomeshelf.Executor.Tests.TestUtilities;
using Tomeshelf.ServiceDefaults;

namespace Tomeshelf.Executor.Tests.Controllers.HomeControllerTests;

public class GetDiscoveredEndpoints
{
    [Fact]
    public async Task ReturnsMappedPayload()
    {
        var controller = HomeControllerTestHarness.CreateController(new ExecutorOptions(), new List<ApiServiceDescriptor>(), out _, out _, out var discovery, out _);
        var endpoints = new List<ExecutorDiscoveredEndpoint> { new("id", "POST", "/path", "Display", "Desc", true, "Group") };
        A.CallTo(() => discovery.GetEndpointsAsync("https://api.test", A<CancellationToken>._))
         .Returns(endpoints);

        var result = await controller.GetDiscoveredEndpoints("https://api.test", CancellationToken.None);

        var ok = result.ShouldBeOfType<OkObjectResult>();
        var payload = ok.Value.ShouldBeAssignableTo<IEnumerable<object>>();
        var item = payload.ShouldHaveSingleItem();
        var itemType = item.GetType();
        itemType.GetProperty("id")!.GetValue(item)
                .ShouldBe("id");
        itemType.GetProperty("groupName")!.GetValue(item)
                .ShouldBe("Group");
    }

    [Fact]
    public async Task WithEmptyBase_ReturnsBadRequest()
    {
        var controller = HomeControllerTestHarness.CreateController(new ExecutorOptions(), new List<ApiServiceDescriptor>(), out _, out _, out _, out _);

        var result = await controller.GetDiscoveredEndpoints(" ", CancellationToken.None);

        result.ShouldBeOfType<BadRequestObjectResult>();
    }
}