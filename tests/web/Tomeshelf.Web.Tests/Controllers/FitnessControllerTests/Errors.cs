using System;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Tomeshelf.Web.Controllers;
using Tomeshelf.Web.Models.Fitness;
using Tomeshelf.Web.Services;

namespace Tomeshelf.Web.Tests.Controllers.FitnessControllerTests;

public class Errors
{
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
        var redirect = result.Should()
                             .BeOfType<RedirectResult>()
                             .Subject;
        redirect.Url
                .Should()
                .Be(location.ToString());
    }

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
        var view = result.Should()
                         .BeOfType<ViewResult>()
                         .Subject;
        var model = view.Model
                        .Should()
                        .BeOfType<FitnessDashboardViewModel>()
                        .Subject;
        model.ErrorMessage
             .Should()
             .Be("service down");
        model.Unit
             .Should()
             .Be(WeightUnit.Kilograms);
    }

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
        var view = result.Should()
                         .BeOfType<ViewResult>()
                         .Subject;
        var model = view.Model
                        .Should()
                        .BeOfType<FitnessDashboardViewModel>()
                        .Subject;
        model.ErrorMessage
             .Should()
             .Be("Unable to load Fitbit data at this time.");
    }

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