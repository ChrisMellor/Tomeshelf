using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Shouldly;
using Tomeshelf.Web.Controllers;
using Tomeshelf.Web.Models.Bundles;
using Tomeshelf.Web.Models.Fitness;
using Tomeshelf.Web.Models.Home;
using Tomeshelf.Web.Models.Mcm;
using Tomeshelf.Web.Models.Paissa;
using Tomeshelf.Web.Services;

namespace Tomeshelf.Web.Tests.Controllers.HomeControllerTests;

public class Index
{
    /// <summary>
    ///     Builds the summary strings.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task BuildsSummaryStrings()
    {
        // Arrange
        var bundlesApi = A.Fake<IBundlesApi>();
        var fitbitApi = A.Fake<IFitbitApi>();
        var guestsApi = A.Fake<IGuestsApi>();
        var paissaApi = A.Fake<IPaissaApi>();
        var logger = A.Fake<ILogger<HomeController>>();

        A.CallTo(() => guestsApi.GetComicConEventsAsync(A<CancellationToken>._))
         .Returns(new List<McmEventConfigModel> { new() });
        A.CallTo(() => bundlesApi.GetBundlesAsync(false, A<CancellationToken>._))
         .Returns(new List<BundleModel>
          {
              new(),
              new()
          });
        A.CallTo(() => fitbitApi.GetDashboardAsync(A<string>._, A<bool>._, A<string>._, A<CancellationToken>._))
         .Returns(Task.FromResult(new FitbitDashboardModel { Activity = new FitbitActivityModel { Steps = 123 } }));

        var world = new PaissaWorldModel(12, "Zalera", DateTimeOffset.UtcNow, new List<PaissaDistrictModel>
        {
            new(1, "Mist", new List<PaissaSizeGroupModel>
            {
                new("S", "s", new List<PaissaPlotModel>
                {
                    new(1, 1, 0, 0, DateTimeOffset.UtcNow, true, true, false),
                    new(1, 2, 0, 0, DateTimeOffset.UtcNow, true, true, false)
                })
            }),
            new(2, "Goblet", new List<PaissaSizeGroupModel> { new("M", "m", new List<PaissaPlotModel> { new(2, 1, 0, 0, DateTimeOffset.UtcNow, true, true, false) }) })
        });

        A.CallTo(() => paissaApi.GetWorldAsync(A<CancellationToken>._))
         .Returns(world);

        var controller = CreateController(bundlesApi, fitbitApi, guestsApi, paissaApi, logger);

        // Act
        var result = await controller.Index(CancellationToken.None);

        // Assert
        var view = result.ShouldBeOfType<ViewResult>();
        var model = view.Model.ShouldBeOfType<HomeIndexViewModel>();
        model.EventsSummary.ShouldBe("1 event configured");
        model.EducationSummary.ShouldBe("2 bundles live");
        model.HealthSummary.ShouldBe("123 steps today");
        model.GamingSummary.ShouldBe("3 plots listed");
    }

    /// <summary>
    ///     Shows unavailable summary when the bundles API fails.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task WhenBundlesApiFails_ShowsUnavailableSummary()
    {
        // Arrange
        var bundlesApi = A.Fake<IBundlesApi>();
        var fitbitApi = A.Fake<IFitbitApi>();
        var guestsApi = A.Fake<IGuestsApi>();
        var paissaApi = A.Fake<IPaissaApi>();
        var logger = A.Fake<ILogger<HomeController>>();

        A.CallTo(() => guestsApi.GetComicConEventsAsync(A<CancellationToken>._))
         .Returns(new List<McmEventConfigModel>());
        A.CallTo(() => bundlesApi.GetBundlesAsync(false, A<CancellationToken>._))
         .Throws(new Exception("boom"));
        A.CallTo(() => fitbitApi.GetDashboardAsync(A<string>._, A<bool>._, A<string>._, A<CancellationToken>._))
         .Returns(Task.FromResult(new FitbitDashboardModel()));
        A.CallTo(() => paissaApi.GetWorldAsync(A<CancellationToken>._))
         .Returns(new PaissaWorldModel(1, "World", DateTimeOffset.UtcNow, new List<PaissaDistrictModel>()));

        var controller = CreateController(bundlesApi, fitbitApi, guestsApi, paissaApi, logger);

        // Act
        var result = await controller.Index(CancellationToken.None);

        // Assert
        var view = result.ShouldBeOfType<ViewResult>();
        var model = view.Model.ShouldBeOfType<HomeIndexViewModel>();
        model.EducationSummary.ShouldBe("Bundles unavailable");
    }

    /// <summary>
    ///     Shows connect message when the fitbit authorization required.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task WhenFitbitAuthorizationRequired_ShowsConnectMessage()
    {
        // Arrange
        var bundlesApi = A.Fake<IBundlesApi>();
        var fitbitApi = A.Fake<IFitbitApi>();
        var guestsApi = A.Fake<IGuestsApi>();
        var paissaApi = A.Fake<IPaissaApi>();
        var logger = A.Fake<ILogger<HomeController>>();

        A.CallTo(() => guestsApi.GetComicConEventsAsync(A<CancellationToken>._))
         .Returns(new List<McmEventConfigModel>());
        A.CallTo(() => bundlesApi.GetBundlesAsync(false, A<CancellationToken>._))
         .Returns(new List<BundleModel>());
        A.CallTo(() => paissaApi.GetWorldAsync(A<CancellationToken>._))
         .Returns(new PaissaWorldModel(1, "World", DateTimeOffset.UtcNow, new List<PaissaDistrictModel>()));
        A.CallTo(() => fitbitApi.GetDashboardAsync(A<string>._, A<bool>._, A<string>._, A<CancellationToken>._))
         .Throws(new FitbitAuthorizationRequiredException(new Uri("https://example.test/fitbit/auth")));

        var controller = CreateController(bundlesApi, fitbitApi, guestsApi, paissaApi, logger);

        // Act
        var result = await controller.Index(CancellationToken.None);

        // Assert
        var view = result.ShouldBeOfType<ViewResult>();
        var model = view.Model.ShouldBeOfType<HomeIndexViewModel>();
        model.HealthSummary.ShouldBe("Connect Fitbit to sync");
    }

    /// <summary>
    ///     Creates the controller.
    /// </summary>
    /// <param name="bundlesApi">The bundles api.</param>
    /// <param name="fitbitApi">The fitbit api.</param>
    /// <param name="guestsApi">The guests api.</param>
    /// <param name="paissaApi">The paissa api.</param>
    /// <param name="logger">The logger.</param>
    /// <returns>The result of the operation.</returns>
    private static HomeController CreateController(IBundlesApi bundlesApi, IFitbitApi fitbitApi, IGuestsApi guestsApi, IPaissaApi paissaApi, ILogger<HomeController> logger)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Scheme = "https";
        httpContext.Request.Host = new HostString("example.test");

        var urlHelper = A.Fake<IUrlHelper>();
        A.CallTo(() => urlHelper.Action(A<UrlActionContext>._))
         .Returns("https://example.test/");
        A.CallTo(() => urlHelper.ActionContext)
         .Returns(new ActionContext(httpContext, new RouteData(), new ActionDescriptor()));

        var controller = new HomeController(bundlesApi, fitbitApi, guestsApi, paissaApi, logger)
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext },
            Url = urlHelper
        };

        return controller;
    }
}