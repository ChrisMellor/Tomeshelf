using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using Shouldly;
using Tomeshelf.Web.Controllers;
using Tomeshelf.Web.Models.Paissa;
using Tomeshelf.Web.Services;

namespace Tomeshelf.Web.Tests.Controllers.PaissaControllerTests;

public class Index
{
    [Fact]
    public async Task AggregatesPlotTotals()
    {
        // Arrange
        var api = A.Fake<IPaissaApi>();
        var world = new PaissaWorldModel(7, "Cerberus", DateTimeOffset.UtcNow, new List<PaissaDistrictModel>
        {
            new(1, "Goblet", new List<PaissaSizeGroupModel>
            {
                new("S", "s", new List<PaissaPlotModel>
                {
                    new(1, 1, 0, 0, DateTimeOffset.UtcNow, true, true, false),
                    new(1, 2, 0, 0, DateTimeOffset.UtcNow, true, true, false)
                }),
                new("M", "m", new List<PaissaPlotModel> { new(1, 3, 0, 0, DateTimeOffset.UtcNow, true, true, false) })
            })
        });

        A.CallTo(() => api.GetWorldAsync(A<CancellationToken>._))
         .Returns(world);

        var controller = new PaissaController(api);

        // Act
        var result = await controller.Index(CancellationToken.None);

        // Assert
        var view = result.ShouldBeOfType<ViewResult>();
        var model = view.Model.ShouldBeOfType<PaissaIndexViewModel>();
        model.TotalPlotCount.ShouldBe(3);
        model.WorldId.ShouldBe(7);
        model.WorldName.ShouldBe("Cerberus");
        model.Districts.ShouldBeSameAs(world.Districts);
    }
}