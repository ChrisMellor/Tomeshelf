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
    [Fact]
    public async Task ReturnsGroupedGuestsAndViewBagStats()
    {
        var api = A.Fake<IGuestsApi>();
        var groups = new List<GuestsGroupModel> { new() { Items = new List<PersonModel>() } };
        var resultModel = new GuestsByEventResult(groups, 42);
        A.CallTo(() => api.GetComicConGuestsByEventResultAsync("mcm-2026", A<CancellationToken>._))
         .Returns(resultModel);

        var controller = new ComicConController(api);

        var result = await controller.Index("mcm-2026", null, CancellationToken.None);

        var view = result.ShouldBeOfType<ViewResult>();
        view.ViewName.ShouldBe("Index");
        view.Model.ShouldBeSameAs(groups);
        ((string)controller.ViewBag.EventName).ShouldBe("mcm-2026");
        ((int)controller.ViewBag.Total).ShouldBe(42);
        ((long)controller.ViewBag.ElapsedMs >= 0).ShouldBeTrue();
    }
}