using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Web.Controllers;
using Tomeshelf.Web.Models.ComicCon;
using Tomeshelf.Web.Services;

namespace Tomeshelf.Web.Tests.Controllers.ComicConControllerTests;

public class ComicConControllerTests
{
    [Fact]
    public async Task Index_ReturnsView_WithModelAndViewBag()
    {
        // Arrange
        var api = A.Fake<IGuestsApi>();
        var groups = new List<GuestsGroupModel> { new GuestsGroupModel { Items = new List<PersonModel>() } };
        A.CallTo(() => api.GetComicConGuestsByCityAsync("London", A<CancellationToken>._))
           .Returns((groups, 1));

        var controller = new ComicConController(api);

        // Act
        var result = await controller.Index("London", TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.ViewName.Should().Be("Index");
        viewResult.Model.Should().BeSameAs(groups);
        ((string)controller.ViewBag.City).Should().Be("London");
        ((int)controller.ViewBag.Total).Should().Be(1);
    }
}
