using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using Shouldly;
using Tomeshelf.Application.Shared.Abstractions.Messaging;
using Tomeshelf.Paissa.Api.Contracts;
using Tomeshelf.Paissa.Api.Controllers;
using Tomeshelf.Paissa.Application.Features.Housing.Dtos;
using Tomeshelf.Paissa.Application.Features.Housing.Queries;

namespace Tomeshelf.Paissa.Api.Tests.Controllers.PaissaControllerTests;

public class GetWorld
{
    [Fact]
    public async Task MapsResponse()
    {
        // Arrange
        var handler = A.Fake<IQueryHandler<GetAcceptingEntriesQuery, PaissaWorldSummaryDto>>();
        var retrievedAt = new DateTimeOffset(2025, 01, 01, 12, 00, 00, TimeSpan.Zero);
        var plots = new List<PaissaPlotSummaryDto> { new PaissaPlotSummaryDto(1, 2, 12345, 4, retrievedAt, true, false, false) };
        var sizeGroups = new List<PaissaSizeGroupSummaryDto> { new PaissaSizeGroupSummaryDto("Small", "small", plots) };
        var districts = new List<PaissaDistrictSummaryDto> { new PaissaDistrictSummaryDto(10, "Mist", sizeGroups) };
        var world = new PaissaWorldSummaryDto(33, "TestWorld", retrievedAt, districts);

        A.CallTo(() => handler.Handle(A<GetAcceptingEntriesQuery>._, A<CancellationToken>._))
         .Returns(Task.FromResult(world));

        var controller = new PaissaController(handler);

        // Act
        var result = await controller.GetWorld(CancellationToken.None);

        // Assert
        var ok = result.Result.ShouldBeOfType<OkObjectResult>();
        var response = ok.Value.ShouldBeOfType<PaissaWorldResponse>();
        response.WorldId.ShouldBe(33);
        response.WorldName.ShouldBe("TestWorld");
        response.Districts.ShouldHaveSingleItem();
        response.Districts[0]
                .Name
                .ShouldBe("Mist");
        response.Districts[0]
                .Tabs
                .ShouldHaveSingleItem();
        response.Districts[0]
                .Tabs[0]
                .Plots
                .ShouldHaveSingleItem();
        response.Districts[0]
                .Tabs[0]
                .Plots[0]
                .Ward
                .ShouldBe(1);
        response.Districts[0]
                .Tabs[0]
                .Plots[0]
                .Plot
                .ShouldBe(2);
    }
}