using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Tomeshelf.Application.Contracts;
using Tomeshelf.Infrastructure.Persistence;
using Tomeshelf.Infrastructure.Queries;
using Tomeshelf.Infrastructure.Services;

namespace Tomeshelf.ComicConApi.Tests.Controllers.ComicConControllerTests;

public class ComicConControllerUpdateGuestsTests
{
    [Fact]
    public async Task UpdateGuests_WhenCityValid_ReturnsOk()
    {
        // Arrange
        var svc = A.Fake<IGuestService>();
        A.CallTo(() => svc.GetGuestsAsync("London", A<CancellationToken>._))
         .Returns(new List<PersonDto>());

        using var db = new TomeshelfMcmDbContext(new DbContextOptionsBuilder<TomeshelfMcmDbContext>().UseInMemoryDatabase(Guid.NewGuid()
                                                                                                                              .ToString())
                                                                                                     .Options);
        var queries = new GuestQueries(db, NullLogger<GuestQueries>.Instance);
        var cache = A.Fake<IGuestsCache>();
        var controller = new ComicConController(svc, queries, NullLogger<ComicConController>.Instance, cache);

        // Act
        var result = await controller.UpdateGuests(City.London, TestContext.Current.CancellationToken);

        // Assert
        result.Should()
              .BeOfType<OkObjectResult>();
        A.CallTo(() => svc.GetGuestsAsync("London", A<CancellationToken>._))
         .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task UpdateGuests_WhenServiceThrowsAppException_ReturnsNotFound()
    {
        // Arrange
        var svc = A.Fake<IGuestService>();
        A.CallTo(() => svc.GetGuestsAsync("Birmingham", A<CancellationToken>._))
         .Throws(new ApplicationException("nope"));
        using var db = new TomeshelfMcmDbContext(new DbContextOptionsBuilder<TomeshelfMcmDbContext>().UseInMemoryDatabase(Guid.NewGuid()
                                                                                                                              .ToString())
                                                                                                     .Options);
        var queries = new GuestQueries(db, NullLogger<GuestQueries>.Instance);
        var cache = A.Fake<IGuestsCache>();
        var controller = new ComicConController(svc, queries, NullLogger<ComicConController>.Instance, cache);

        // Act
        var result = await controller.UpdateGuests(City.Birmingham, TestContext.Current.CancellationToken);

        // Assert
        result.Should()
              .BeOfType<NotFoundObjectResult>();
        A.CallTo(() => svc.GetGuestsAsync("Birmingham", A<CancellationToken>._))
         .MustHaveHappenedOnceExactly();
    }
}