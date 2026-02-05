using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
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
        // Arrange
        var api = A.Fake<IGuestsApi>();
        var groups = new List<GuestsGroupModel> { new GuestsGroupModel { Items = new List<PersonModel>() } };
        var resultModel = new GuestsByEventResult(groups, 42);
        A.CallTo(() => api.GetComicConGuestsByEventResultAsync("mcm-2026", A<CancellationToken>._))
         .Returns(resultModel);

        var controller = new ComicConController(api);

        // Act
        var result = await controller.Index("mcm-2026", null, CancellationToken.None);

        // Assert
        var view = result.Should()
                         .BeOfType<ViewResult>()
                         .Subject;
        view.ViewName
            .Should()
            .Be("Index");
        view.Model
            .Should()
            .BeEquivalentTo(groups);
        ((string)controller.ViewBag.EventName).Should()
                                              .Be("mcm-2026");
        ((int)controller.ViewBag.Total).Should()
                                       .Be(42);
        ((long)controller.ViewBag.ElapsedMs).Should()
                                            .BeGreaterThanOrEqualTo(0);
    }
}