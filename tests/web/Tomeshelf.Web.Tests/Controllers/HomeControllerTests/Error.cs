using System.Diagnostics;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Tomeshelf.Web.Controllers;
using Tomeshelf.Web.Models;
using Tomeshelf.Web.Services;

namespace Tomeshelf.Web.Tests.Controllers.HomeControllerTests;

public class Error
{
    [Fact]
    public void UsesTraceIdentifierWhenNoActivity()
    {
        // Arrange
        var bundlesApi = A.Fake<IBundlesApi>();
        var fitbitApi = A.Fake<IFitbitApi>();
        var guestsApi = A.Fake<IGuestsApi>();
        var paissaApi = A.Fake<IPaissaApi>();
        var logger = A.Fake<ILogger<HomeController>>();

        var httpContext = new DefaultHttpContext { TraceIdentifier = "trace-123" };
        var controller = new HomeController(bundlesApi, fitbitApi, guestsApi, paissaApi, logger) { ControllerContext = new ControllerContext { HttpContext = httpContext } };

        var previous = Activity.Current;
        Activity.Current = null;

        try
        {
            // Act
            var result = controller.Error();

            // Assert
            var view = result.Should()
                             .BeOfType<ViewResult>()
                             .Subject;
            var model = view.Model
                            .Should()
                            .BeOfType<ErrorViewModel>()
                            .Subject;
            model.RequestId
                 .Should()
                 .Be("trace-123");
            model.ShowRequestId
                 .Should()
                 .BeTrue();
        }
        finally
        {
            Activity.Current = previous;
        }
    }
}