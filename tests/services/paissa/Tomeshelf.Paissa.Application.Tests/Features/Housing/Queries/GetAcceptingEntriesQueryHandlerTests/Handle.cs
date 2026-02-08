using Bogus;
using FakeItEasy;
using Shouldly;
using Tomeshelf.Paissa.Application.Abstractions.Common;
using Tomeshelf.Paissa.Application.Abstractions.External;
using Tomeshelf.Paissa.Application.Features.Housing.Queries;
using Tomeshelf.Paissa.Domain.Entities;
using Tomeshelf.Paissa.Domain.ValueObjects;

namespace Tomeshelf.Paissa.Application.Tests.Features.Housing.Queries.GetAcceptingEntriesQueryHandlerTests;

public class Handle
{
    [Fact]
    public async Task FiltersUnknownSizes_WhenRequireKnownSize()
    {
        // Arrange
        var client = A.Fake<IPaissaClient>();
        var settings = A.Fake<IPaissaWorldSettings>();
        var clock = A.Fake<IClock>();
        var handler = new GetAcceptingEntriesQueryHandler(client, settings, clock);

        var now = DateTimeOffset.UtcNow;
        A.CallTo(() => settings.WorldId)
         .Returns(3);
        A.CallTo(() => clock.UtcNow)
         .Returns(now);

        var plotUnknown = PaissaPlot.Create(1, 1, HousingPlotSize.Unknown, 100, now, PurchaseSystem.Personal, 1, LotteryPhase.AcceptingEntries);
        var plotKnown = PaissaPlot.Create(1, 2, HousingPlotSize.Small, 200, now, PurchaseSystem.Personal, 1, LotteryPhase.AcceptingEntries);

        var districtUnknown = PaissaDistrict.Create(1, "Unknown", new List<PaissaPlot> { plotUnknown });
        var districtKnown = PaissaDistrict.Create(2, "Known", new List<PaissaPlot> { plotKnown });
        var world = PaissaWorld.Create(3, "World-3", new List<PaissaDistrict>
        {
            districtUnknown,
            districtKnown
        });

        A.CallTo(() => client.GetWorldAsync(3, A<CancellationToken>._))
         .Returns(Task.FromResult(world));

        // Act
        var result = await handler.Handle(new GetAcceptingEntriesQuery(), CancellationToken.None);

        // Assert
        result.Districts.ShouldHaveSingleItem();
        result.Districts[0]
              .Name
              .ShouldBe("Known");
    }

    [Fact]
    public async Task OrdersDistrictsAndGroupsBySize()
    {
        // Arrange
        var client = A.Fake<IPaissaClient>();
        var settings = A.Fake<IPaissaWorldSettings>();
        var clock = A.Fake<IClock>();
        var handler = new GetAcceptingEntriesQueryHandler(client, settings, clock);

        var now = DateTimeOffset.UtcNow;
        A.CallTo(() => settings.WorldId)
         .Returns(7);
        A.CallTo(() => clock.UtcNow)
         .Returns(now);

        var plotLarge = PaissaPlot.Create(2, 10, HousingPlotSize.Large, 100, now, PurchaseSystem.Personal, 1, LotteryPhase.AcceptingEntries);
        var plotSmall = PaissaPlot.Create(1, 2, HousingPlotSize.Small, 200, now, PurchaseSystem.Personal, 2, LotteryPhase.AcceptingEntries);
        var plotMedium = PaissaPlot.Create(1, 1, HousingPlotSize.Medium, 300, now, PurchaseSystem.FreeCompany, 3, LotteryPhase.AcceptingEntries);

        var districtBeta = PaissaDistrict.Create(2, "beta", new List<PaissaPlot>
        {
            plotLarge,
            plotSmall
        });
        var districtAlpha = PaissaDistrict.Create(1, "Alpha", new List<PaissaPlot> { plotMedium });
        var world = PaissaWorld.Create(7, "World-7", new List<PaissaDistrict>
        {
            districtBeta,
            districtAlpha
        });

        A.CallTo(() => client.GetWorldAsync(7, A<CancellationToken>._))
         .Returns(Task.FromResult(world));

        // Act
        var result = await handler.Handle(new GetAcceptingEntriesQuery(), CancellationToken.None);

        // Assert
        result.Districts.Count.ShouldBe(2);
        result.Districts[0]
              .Name
              .ShouldBe("Alpha");
        result.Districts[1]
              .Name
              .ShouldBe("beta");

        var sizeGroups = result.Districts[0].SizeGroups;
        sizeGroups.Count.ShouldBe(3);
        sizeGroups[0]
           .Size
           .ShouldBe("Large");
        sizeGroups[1]
           .Size
           .ShouldBe("Medium");
        sizeGroups[2]
           .Size
           .ShouldBe("Small");

        var mediumGroup = sizeGroups[1];
        mediumGroup.Plots.ShouldHaveSingleItem();
        mediumGroup.Plots[0]
                   .Ward
                   .ShouldBe(1);
        mediumGroup.Plots[0]
                   .Plot
                   .ShouldBe(1);

        var betaSmallPlots = result.Districts[1]
                                   .SizeGroups
                                   .First(group => group.Size == "Small")
                                   .Plots;
        betaSmallPlots.ShouldHaveSingleItem();
        betaSmallPlots[0]
           .Ward
           .ShouldBe(1);
        betaSmallPlots[0]
           .Plot
           .ShouldBe(2);
    }

    [Fact]
    public async Task ValidQuery_ReturnsPaissaWorldSummary()
    {
        // Arrange
        var faker = new Faker();
        var client = A.Fake<IPaissaClient>();
        var settings = A.Fake<IPaissaWorldSettings>();
        var clock = A.Fake<IClock>();
        var handler = new GetAcceptingEntriesQueryHandler(client, settings, clock);

        var worldId = faker.Random.Int(1, 99);
        var worldName = faker.Company.CompanyName();
        var now = faker.Date.RecentOffset();

        A.CallTo(() => settings.WorldId)
         .Returns(worldId);
        A.CallTo(() => clock.UtcNow)
         .Returns(now);

        var plots = new List<PaissaPlot> { PaissaPlot.Create(1, 1, HousingPlotSize.Small, 1000, now, PurchaseSystem.FreeCompany, 5, LotteryPhase.AcceptingEntries) };
        var districts = new List<PaissaDistrict> { PaissaDistrict.Create(1, "Test District", plots) };
        var world = PaissaWorld.Create(worldId, worldName, districts);

        A.CallTo(() => client.GetWorldAsync(worldId, A<CancellationToken>._))
         .Returns(Task.FromResult(world));

        // Act
        var result = await handler.Handle(new GetAcceptingEntriesQuery(), CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.WorldId.ShouldBe(worldId);
        result.WorldName.ShouldBe(worldName);
        result.RetrievedAtUtc.ShouldBe(now);
        result.Districts.ShouldHaveSingleItem();
        A.CallTo(() => client.GetWorldAsync(worldId, A<CancellationToken>._))
         .MustHaveHappenedOnceExactly();
    }
}