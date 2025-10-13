using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Threading.Tasks;
using Tomeshelf.ComicConApi.Controllers;
using Tomeshelf.ComicConApi.Services;
using Tomeshelf.Infrastructure.Persistence;
using Tomeshelf.Infrastructure.Queries;
using Tomeshelf.Infrastructure.Services;

namespace Tomeshelf.ComicConApi.Tests.Controllers.ComicConControllerTests;

public class ComicConControllerGetByCityTests
{
    [Fact]
    public async Task GetByCity_WhenCityMissing_ReturnsBadRequest()
    {
        // Arrange
        var svc = A.Fake<IGuestService>();
        using var db = new TomeshelfDbContext(new DbContextOptionsBuilder<TomeshelfDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var queries = new GuestQueries(db, NullLogger<GuestQueries>.Instance);
        var cache = A.Fake<IGuestsCache>();
        var controller = new ComicConController(svc, queries, NullLogger<ComicConController>.Instance, cache);

        // Act
        var result = await controller.GetByCity(string.Empty, TestContext.Current.CancellationToken);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task GetByCity_ReturnsOk_WithTotal()
    {
        // Arrange
        using var db = new TomeshelfDbContext(new DbContextOptionsBuilder<TomeshelfDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

        var ev = new Domain.Entities.ComicCon.Event
        {
            ExternalId = Guid.NewGuid().ToString(),
            Name = "Event",
            Slug = "2025-london"
        };
        var p = new Domain.Entities.ComicCon.Person
        {
            ExternalId = Guid.NewGuid().ToString(),
            FirstName = "Ada",
            LastName = "Lovelace"
        };
        db.Events.Add(ev);
        db.People.Add(p);
        db.EventAppearances.Add(new Domain.Entities.ComicCon.EventAppearance { Event = ev, Person = p });
        await db.SaveChangesAsync();

        var queries = new GuestQueries(db, NullLogger<GuestQueries>.Instance);
        var svc = A.Fake<IGuestService>();
        var cache = A.Fake<IGuestsCache>();
        var controller = new ComicConController(svc, queries, NullLogger<ComicConController>.Instance, cache);

        // Act
        var result = await controller.GetByCity("London", TestContext.Current.CancellationToken);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result.Result!;
        var okPayload = okResult.Value!;
        okPayload.Should().BeEquivalentTo(new { city = "London", total = 1 }, options => options.ExcludingMissingMembers());
    }

}

