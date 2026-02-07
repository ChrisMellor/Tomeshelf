using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using Shouldly;
using Tomeshelf.Executor.Configuration;
using Tomeshelf.Executor.Models;
using Tomeshelf.Executor.Services;
using Tomeshelf.Executor.Tests.TestUtilities;

namespace Tomeshelf.Executor.Tests.Controllers.HomeControllerTests;

public class Ping
{
    [Fact]
    public async Task Get_ReturnsPingDefaults()
    {
        var controller = HomeControllerTestHarness.CreateController(new ExecutorOptions(), new List<ApiServiceDescriptor>(), out _, out _, out _, out _);

        var result = await controller.Ping(CancellationToken.None);

        var view = result.ShouldBeOfType<ViewResult>();
        var model = view.Model.ShouldBeOfType<ExecutorConfigurationViewModel>();
        model.Ping.Method.ShouldBe("GET");
    }

    [Fact]
    public async Task Post_InvalidUrl_ReturnsViewWithErrors()
    {
        var controller = HomeControllerTestHarness.CreateController(new ExecutorOptions(), new List<ApiServiceDescriptor>(), out _, out _, out _, out var pingService);

        var model = new EndpointPingModel
        {
            Url = "not-a-url",
            Method = "GET"
        };

        var result = await controller.Ping(model, CancellationToken.None);

        var view = result.ShouldBeOfType<ViewResult>();
        view.ViewName.ShouldBe("Ping");
        A.CallTo(() => pingService.SendAsync(A<Uri>._, A<string>._, A<Dictionary<string, string>?>._, A<CancellationToken>._))
         .MustNotHaveHappened();
        controller.ModelState[nameof(EndpointPingModel.Url)]!.Errors.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task Post_Success_ReturnsResultAndStatusMessage()
    {
        var controller = HomeControllerTestHarness.CreateController(new ExecutorOptions(), new List<ApiServiceDescriptor>(), out _, out _, out _, out var pingService);
        var body = new string('a', 2100);
        A.CallTo(() => pingService.SendAsync(A<Uri>._, A<string>._, A<Dictionary<string, string>?>._, A<CancellationToken>._))
         .Returns(new EndpointPingResult(true, 200, "OK", body, TimeSpan.FromMilliseconds(5)));

        var model = new EndpointPingModel
        {
            Url = "https://example.test",
            Method = "GET",
            Headers = "X-Test: value"
        };

        var result = await controller.Ping(model, CancellationToken.None);

        var view = result.ShouldBeOfType<ViewResult>();
        var viewModel = view.Model.ShouldBeOfType<ExecutorConfigurationViewModel>();
        viewModel.PingResult.ShouldNotBeNull();
        viewModel.PingResult!.ResponseBody.ShouldEndWith("... (truncated)");
        controller.TempData["StatusMessage"]
                  .ShouldBe("Ping succeeded with status 200 (5 ms).");
    }
}