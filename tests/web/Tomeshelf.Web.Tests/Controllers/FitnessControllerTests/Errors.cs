using System;
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
using Tomeshelf.Web.Models.Fitness;
using Tomeshelf.Web.Services;

namespace Tomeshelf.Web.Tests.Controllers.FitnessControllerTests;

public class Errors
{
    /// <summary>
    ///     Redirects when the authorization required.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task WhenAuthorizationRequired_Redirects()
    {
        // Arrange
        var api = A.Fake<IFitbitApi>();
        var logger = A.Fake<ILogger<FitnessController>>();
        var location = new Uri("https://example.test/fitbit/auth");
        A.CallTo(() => api.GetOverviewAsync("2020-01-01", A<bool>._, A<string>._, A<CancellationToken>._))
         .Throws(new FitbitAuthorizationRequiredException(location));

        var controller = CreateController(api, logger);

        // Act
        var result = await controller.Index("2020-01-01", false, "kg", CancellationToken.None);

        // Assert
        var redirect = result.ShouldBeOfType<RedirectResult>();
        redirect.Url.ShouldBe(location.ToString());
    }

    /// <summary>
    ///     Returns error view when the backend is unavailable.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task WhenBackendUnavailable_ReturnsErrorView()
    {
        // Arrange
        var api = A.Fake<IFitbitApi>();
        var logger = A.Fake<ILogger<FitnessController>>();
        A.CallTo(() => api.GetOverviewAsync("2020-01-02", A<bool>._, A<string>._, A<CancellationToken>._))
         .Throws(new FitbitBackendUnavailableException("service down"));

        var controller = CreateController(api, logger);

        // Act
        var result = await controller.Index("2020-01-02", false, "kg", CancellationToken.None);

        // Assert
        var view = result.ShouldBeOfType<ViewResult>();
        var model = view.Model.ShouldBeOfType<FitnessDashboardViewModel>();
        model.ErrorMessage.ShouldBe("service down");
        model.Unit.ShouldBe(WeightUnit.Kilograms);
    }

    /// <summary>
    ///     Returns generic error when the unexpected exception.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task WhenUnexpectedException_ReturnsGenericError()
    {
        // Arrange
        var api = A.Fake<IFitbitApi>();
        var logger = A.Fake<ILogger<FitnessController>>();
        A.CallTo(() => api.GetOverviewAsync("2020-01-03", A<bool>._, A<string>._, A<CancellationToken>._))
         .Throws(new Exception("boom"));

        var controller = CreateController(api, logger);

        // Act
        var result = await controller.Index("2020-01-03", false, "kg", CancellationToken.None);

        // Assert
        var view = result.ShouldBeOfType<ViewResult>();
        var model = view.Model.ShouldBeOfType<FitnessDashboardViewModel>();
        model.ErrorMessage.ShouldBe("Unable to load Fitbit data at this time.");
    }

    /// <summary>
    ///     Creates the controller.
    /// </summary>
    /// <param name="api">The api.</param>
    /// <param name="logger">The logger.</param>
    /// <returns>The result of the operation.</returns>
    private static FitnessController CreateController(IFitbitApi api, ILogger<FitnessController> logger)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Scheme = "https";
        httpContext.Request.Host = new HostString("example.test");

        var urlHelper = A.Fake<IUrlHelper>();
        A.CallTo(() => urlHelper.Action(A<UrlActionContext>._))
         .Returns("https://example.test/fitness");
        A.CallTo(() => urlHelper.ActionContext)
         .Returns(new ActionContext(httpContext, new RouteData(), new ActionDescriptor()));

        return new FitnessController(api, logger)
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext },
            Url = urlHelper
        };
    }
}