using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using FakeItEasy;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Api.Controllers;
using Tomeshelf.Api.Enums;
using Tomeshelf.Application.Contracts;
using Tomeshelf.Infrastructure.Persistence;
using Tomeshelf.Infrastructure.Queries;
using Tomeshelf.Infrastructure.Services;

namespace Tomeshelf.Api.Tests.Controllers.ComicConControllerTests;

public class ComicConController_UpdateGuests_Tests
{
    [Fact]
    public async Task UpdateGuests_WhenCityValid_ReturnsOk()
    {
        // Arrange
        var svc = A.Fake<IGuestService>();
        A.CallTo(() => svc.GetGuests("London")).Returns(new List<PersonDto>());

        using var db = new TomeshelfDbContext(new DbContextOptionsBuilder<TomeshelfDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var queries = new GuestQueries(db, NullLogger<GuestQueries>.Instance);
        var controller = new ComicConController(svc, queries, NullLogger<ComicConController>.Instance);

        // Act
        var result = await controller.UpdateGuests(City.London);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        A.CallTo(() => svc.GetGuests("London")).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task UpdateGuests_WhenServiceThrowsAppException_ReturnsNotFound()
    {
        // Arrange
        var svc = A.Fake<IGuestService>();
        A.CallTo(() => svc.GetGuests("Birmingham")).Throws(new ApplicationException("nope"));
        using var db = new TomeshelfDbContext(new DbContextOptionsBuilder<TomeshelfDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var queries = new GuestQueries(db, NullLogger<GuestQueries>.Instance);
        var controller = new ComicConController(svc, queries, NullLogger<ComicConController>.Instance);

        // Act
        var result = await controller.UpdateGuests(City.Birmingham);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        A.CallTo(() => svc.GetGuests("Birmingham")).MustHaveHappenedOnceExactly();
    }
}

