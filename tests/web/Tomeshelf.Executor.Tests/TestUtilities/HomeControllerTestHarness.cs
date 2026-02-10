using System.Collections.Generic;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Tomeshelf.Executor.Configuration;
using Tomeshelf.Executor.Controllers;
using Tomeshelf.Executor.Services;
using Tomeshelf.ServiceDefaults;

namespace Tomeshelf.Executor.Tests.TestUtilities;

internal static class HomeControllerTestHarness
{
    /// <summary>
    ///     Creates the controller.
    /// </summary>
    /// <param name="options">The options.</param>
    /// <param name="apis">The apis.</param>
    /// <param name="store">The store.</param>
    /// <param name="scheduler">The scheduler.</param>
    /// <param name="discovery">The discovery.</param>
    /// <param name="pingService">The ping service.</param>
    /// <returns>The result of the operation.</returns>
    public static HomeController CreateController(
        ExecutorOptions options,
        IReadOnlyList<ApiServiceDescriptor> apis,
        out IExecutorConfigurationStore store,
        out IExecutorSchedulerOrchestrator scheduler,
        out IApiEndpointDiscoveryService discovery,
        out IEndpointPingService pingService)
    {
        var storeFake = A.Fake<IExecutorConfigurationStore>();
        var schedulerFake = A.Fake<IExecutorSchedulerOrchestrator>();
        var discoveryFake = A.Fake<IApiEndpointDiscoveryService>();
        var pingServiceFake = A.Fake<IEndpointPingService>();
        var logger = A.Fake<ILogger<HomeController>>();

        A.CallTo(() => storeFake.GetAsync(A<CancellationToken>._))
         .Returns(options);
        A.CallTo(() => discoveryFake.GetApisAsync(A<CancellationToken>._))
         .Returns(apis);

        store = storeFake;
        scheduler = schedulerFake;
        discovery = discoveryFake;
        pingService = pingServiceFake;

        var controller = new HomeController(storeFake, schedulerFake, discoveryFake, pingServiceFake, logger);
        var httpContext = new DefaultHttpContext();
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        var tempDataProvider = A.Fake<ITempDataProvider>();
        A.CallTo(() => tempDataProvider.LoadTempData(httpContext))
         .Returns(new Dictionary<string, object?>());
        controller.TempData = new TempDataDictionary(httpContext, tempDataProvider);

        return controller;
    }
}
