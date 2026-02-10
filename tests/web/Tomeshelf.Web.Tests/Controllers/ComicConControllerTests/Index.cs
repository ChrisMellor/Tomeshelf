using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using Shouldly;
using Tomeshelf.Web.Controllers;
using Tomeshelf.Web.Models;
using Tomeshelf.Web.Models.ComicCon;
using Tomeshelf.Web.Services;

namespace Tomeshelf.Web.Tests.Controllers.ComicConControllerTests;

public class Index
{
    /// <summary>
    ///     Returns the grouped guests and view bag stats.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task ReturnsGroupedGuestsAndViewBagStats()
    {
        // Arrange
        var api = A.Fake<IGuestsApi>();
        var groups = new List<GuestsGroupModel> { new() { Items = new List<PersonModel>() } };
        var resultModel = new GuestsByEventResult(groups, 42);
        A.CallTo(() => api.GetComicConGuestsByEventResultAsync("mcm-2026", A<CancellationToken>._))
         .Returns(resultModel);

        var controller = new ComicConController(api);

        // Act
        var result = await controller.Index("mcm-2026", null, CancellationToken.None);

        // Assert
        var view = result.ShouldBeOfType<ViewResult>();
        view.ViewName.ShouldBe("Index");
        view.Model.ShouldBeSameAs(groups);
        ((string)controller.ViewBag.EventName).ShouldBe("mcm-2026");
        ((int)controller.ViewBag.Total).ShouldBe(42);
        ((long)controller.ViewBag.ElapsedMs >= 0).ShouldBeTrue();
    }
}