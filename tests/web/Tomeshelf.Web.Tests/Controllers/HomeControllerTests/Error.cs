using System.Diagnostics;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shouldly;
using Tomeshelf.Web.Controllers;
using Tomeshelf.Web.Models;
using Tomeshelf.Web.Services;

namespace Tomeshelf.Web.Tests.Controllers.HomeControllerTests;

public class Error
{
    [Fact]
    public void UsesTraceIdentifierWhenNoActivity()
    {
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
            var result = controller.Error();

            var view = result.ShouldBeOfType<ViewResult>();
            var model = view.Model.ShouldBeOfType<ErrorViewModel>();
            model.RequestId.ShouldBe("trace-123");
            model.ShowRequestId.ShouldBeTrue();
        }
        finally
        {
            Activity.Current = previous;
        }
    }
}