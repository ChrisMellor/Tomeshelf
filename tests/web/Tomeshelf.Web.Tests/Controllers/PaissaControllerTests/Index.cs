using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
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
            new PaissaDistrictModel(1, "Goblet", new List<PaissaSizeGroupModel>
            {
                new PaissaSizeGroupModel("S", "s", new List<PaissaPlotModel>
                {
                    new PaissaPlotModel(1, 1, 0, 0, DateTimeOffset.UtcNow, true, true, false),
                    new PaissaPlotModel(1, 2, 0, 0, DateTimeOffset.UtcNow, true, true, false)
                }),
                new PaissaSizeGroupModel("M", "m", new List<PaissaPlotModel> { new PaissaPlotModel(1, 3, 0, 0, DateTimeOffset.UtcNow, true, true, false) })
            })
        });

        A.CallTo(() => api.GetWorldAsync(A<CancellationToken>._))
         .Returns(world);

        var controller = new PaissaController(api);

        // Act
        var result = await controller.Index(CancellationToken.None);

        // Assert
        var view = result.Should()
                         .BeOfType<ViewResult>()
                         .Subject;
        var model = view.Model
                        .Should()
                        .BeOfType<PaissaIndexViewModel>()
                        .Subject;
        model.TotalPlotCount
             .Should()
             .Be(3);
        model.WorldId
             .Should()
             .Be(7);
        model.WorldName
             .Should()
             .Be("Cerberus");
        model.Districts
             .Should()
             .BeSameAs(world.Districts);
    }
}