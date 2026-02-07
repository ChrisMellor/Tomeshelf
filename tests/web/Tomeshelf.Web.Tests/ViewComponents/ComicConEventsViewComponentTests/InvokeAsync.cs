using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.Extensions.Logging;
using Tomeshelf.Web.Models.Mcm;
using Tomeshelf.Web.Services;
using Tomeshelf.Web.ViewComponents;

namespace Tomeshelf.Web.Tests.ViewComponents.ComicConEventsViewComponentTests;

public class InvokeAsync
{
    [Fact]
    public async Task WhenApiReturnsEvents_RendersThem()
    {
        // Arrange
        var api = A.Fake<IGuestsApi>();
        var logger = A.Fake<ILogger<ComicConEventsViewComponent>>();
        var events = new List<McmEventConfigModel>
        {
            new McmEventConfigModel
            {
                Id = "mcm-1",
                Name = "MCM"
            }
        };
        A.CallTo(() => api.GetComicConEventsAsync(A<CancellationToken>._))
         .Returns(events);

        var component = new ComicConEventsViewComponent(api, logger) { ViewComponentContext = CreateViewComponentContext() };

        // Act
        var result = await component.InvokeAsync();

        // Assert
        var view = result.ShouldBeOfType<ViewViewComponentResult>();
        var model = view.ViewData.Model.ShouldBeAssignableTo<IEnumerable<McmEventConfigModel>>();
        var item = model.ShouldHaveSingleItem();
        item.Id.ShouldBe("mcm-1");
        item.Name.ShouldBe("MCM");
    }

    [Fact]
    public async Task WhenApiThrows_ReturnsEmptyList()
    {
        // Arrange
        var api = A.Fake<IGuestsApi>();
        var logger = A.Fake<ILogger<ComicConEventsViewComponent>>();
        A.CallTo(() => api.GetComicConEventsAsync(A<CancellationToken>._))
         .Throws(new InvalidOperationException("boom"));

        var component = new ComicConEventsViewComponent(api, logger) { ViewComponentContext = CreateViewComponentContext() };

        // Act
        var result = await component.InvokeAsync();

        // Assert
        var view = result.ShouldBeOfType<ViewViewComponentResult>();
        var model = view.ViewData.Model.ShouldBeAssignableTo<IEnumerable<McmEventConfigModel>>();
        model.ShouldBeEmpty();
    }

    private static ViewComponentContext CreateViewComponentContext()
    {
        var httpContext = new DefaultHttpContext();
        var viewContext = new ViewContext { HttpContext = httpContext };

        return new ViewComponentContext { ViewContext = viewContext };
    }
}
