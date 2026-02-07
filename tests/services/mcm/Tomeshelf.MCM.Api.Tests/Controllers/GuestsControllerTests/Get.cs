using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using Shouldly;
using Tomeshelf.Application.Shared.Abstractions.Messaging;
using Tomeshelf.MCM.Api.Controllers;
using Tomeshelf.MCM.Application.Contracts;
using Tomeshelf.MCM.Application.Features.Guests.Commands;
using Tomeshelf.MCM.Application.Features.Guests.Queries;

namespace Tomeshelf.MCM.Api.Tests.Controllers.GuestsControllerTests;

public class Get
{
    [Fact]
    public async Task ReturnsBadRequest_WhenPageIsLessThanOne()
    {
        var queryHandler = A.Fake<IQueryHandler<GetGuestsQuery, PagedResult<GuestDto>>>();
        var syncHandler = A.Fake<ICommandHandler<SyncGuestsCommand, GuestSyncResultDto?>>();
        var controller = new GuestsController(queryHandler, syncHandler);

        var result = await controller.Get("event-1", CancellationToken.None, 0);

        var badRequest = result.ShouldBeOfType<ObjectResult>();
        var problem = badRequest.Value.ShouldBeOfType<ValidationProblemDetails>();
        problem.Detail.ShouldBe("page must be >= 1");
    }

    [Fact]
    public async Task ReturnsBadRequest_WhenPageSizeIsOutOfRange()
    {
        var queryHandler = A.Fake<IQueryHandler<GetGuestsQuery, PagedResult<GuestDto>>>();
        var syncHandler = A.Fake<ICommandHandler<SyncGuestsCommand, GuestSyncResultDto?>>();
        var controller = new GuestsController(queryHandler, syncHandler);

        var result = await controller.Get("event-1", CancellationToken.None, 1, 401);

        var badRequest = result.ShouldBeOfType<ObjectResult>();
        var problem = badRequest.Value.ShouldBeOfType<ValidationProblemDetails>();
        problem.Detail.ShouldBe("pageSize must be between 1 and 400");
    }

    [Fact]
    public async Task ReturnsOk_WithPagedResults()
    {
        var queryHandler = A.Fake<IQueryHandler<GetGuestsQuery, PagedResult<GuestDto>>>();
        var syncHandler = A.Fake<ICommandHandler<SyncGuestsCommand, GuestSyncResultDto?>>();
        var controller = new GuestsController(queryHandler, syncHandler);
        var guest = new GuestDto(Guid.NewGuid(), "Guest", "Bio", "https://profile", "https://image", DateTimeOffset.UtcNow, null, false);
        var expected = new PagedResult<GuestDto>(1, new List<GuestDto> { guest }, 2, 25);

        A.CallTo(() => queryHandler.Handle(A<GetGuestsQuery>._, A<CancellationToken>._))
         .Returns(Task.FromResult(expected));

        var result = await controller.Get("event-1", CancellationToken.None, 2, 25, "Event", true);

        var ok = result.ShouldBeOfType<OkObjectResult>();
        ok.Value.ShouldBeSameAs(expected);
        A.CallTo(() => queryHandler.Handle(A<GetGuestsQuery>.That.Matches(query => (query.EventId == "event-1") && (query.Page == 2) && (query.PageSize == 25) && (query.EventName == "Event") && query.IncludeDeleted), A<CancellationToken>._))
         .MustHaveHappenedOnceExactly();
    }
}