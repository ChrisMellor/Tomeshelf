using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Tomeshelf.Application.Shared.Abstractions.Messaging;
using Tomeshelf.MCM.Api.Controllers;
using Tomeshelf.MCM.Application.Contracts;
using Tomeshelf.MCM.Application.Features.Guests.Commands;
using Tomeshelf.MCM.Application.Features.Guests.Queries;

namespace Tomeshelf.MCM.Api.Tests.Controllers;

public class GuestsControllerTests
{
    [Fact]
    public async Task Get_ReturnsBadRequest_WhenPageIsLessThanOne()
    {
        var queryHandler = A.Fake<IQueryHandler<GetGuestsQuery, PagedResult<GuestDto>>>();
        var syncHandler = A.Fake<ICommandHandler<SyncGuestsCommand, GuestSyncResultDto?>>();
        var controller = new GuestsController(queryHandler, syncHandler);

        var result = await controller.Get("event-1", CancellationToken.None, 0, 50);

        var badRequest = result.Should()
                               .BeOfType<ObjectResult>()
                               .Subject;
        var problem = badRequest.Value
                                .Should()
                                .BeOfType<ValidationProblemDetails>()
                                .Subject;
        problem.Detail
               .Should()
               .Be("page must be >= 1");
    }

    [Fact]
    public async Task Get_ReturnsBadRequest_WhenPageSizeIsOutOfRange()
    {
        var queryHandler = A.Fake<IQueryHandler<GetGuestsQuery, PagedResult<GuestDto>>>();
        var syncHandler = A.Fake<ICommandHandler<SyncGuestsCommand, GuestSyncResultDto?>>();
        var controller = new GuestsController(queryHandler, syncHandler);

        var result = await controller.Get("event-1", CancellationToken.None, 1, 401);

        var badRequest = result.Should()
                               .BeOfType<ObjectResult>()
                               .Subject;
        var problem = badRequest.Value
                                .Should()
                                .BeOfType<ValidationProblemDetails>()
                                .Subject;
        problem.Detail
               .Should()
               .Be("pageSize must be between 1 and 400");
    }

    [Fact]
    public async Task Get_ReturnsOk_WithPagedResults()
    {
        var queryHandler = A.Fake<IQueryHandler<GetGuestsQuery, PagedResult<GuestDto>>>();
        var syncHandler = A.Fake<ICommandHandler<SyncGuestsCommand, GuestSyncResultDto?>>();
        var controller = new GuestsController(queryHandler, syncHandler);
        var guest = new GuestDto(Guid.NewGuid(), "Guest", "Bio", "https://profile", "https://image", DateTimeOffset.UtcNow, null, false);
        var expected = new PagedResult<GuestDto>(1, new List<GuestDto> { guest }, 2, 25);

        A.CallTo(() => queryHandler.Handle(A<GetGuestsQuery>._, A<CancellationToken>._))
         .Returns(Task.FromResult(expected));

        var result = await controller.Get("event-1", CancellationToken.None, 2, 25, "Event", true);

        var ok = result.Should()
                       .BeOfType<OkObjectResult>()
                       .Subject;
        ok.Value
          .Should()
          .BeSameAs(expected);

        A.CallTo(() => queryHandler.Handle(A<GetGuestsQuery>.That.Matches(query => (query.EventId == "event-1") && (query.Page == 2) && (query.PageSize == 25) && (query.EventName == "Event") && query.IncludeDeleted), A<CancellationToken>._))
         .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Sync_ReturnsNotFound_WhenSyncResultIsNull()
    {
        var queryHandler = A.Fake<IQueryHandler<GetGuestsQuery, PagedResult<GuestDto>>>();
        var syncHandler = A.Fake<ICommandHandler<SyncGuestsCommand, GuestSyncResultDto?>>();
        var controller = new GuestsController(queryHandler, syncHandler);

        A.CallTo(() => syncHandler.Handle(A<SyncGuestsCommand>._, A<CancellationToken>._))
         .Returns(Task.FromResult<GuestSyncResultDto?>(null));

        var result = await controller.Sync("event-1", CancellationToken.None);

        var notFound = result.Result
                             .Should()
                             .BeOfType<NotFoundObjectResult>()
                             .Subject;
        notFound.Value
                .Should()
                .Be("event-1");
        A.CallTo(() => syncHandler.Handle(A<SyncGuestsCommand>.That.Matches(command => command.EventId == "event-1"), A<CancellationToken>._))
         .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Sync_ReturnsOk_WithSyncResult()
    {
        var queryHandler = A.Fake<IQueryHandler<GetGuestsQuery, PagedResult<GuestDto>>>();
        var syncHandler = A.Fake<ICommandHandler<SyncGuestsCommand, GuestSyncResultDto?>>();
        var controller = new GuestsController(queryHandler, syncHandler);
        var syncResult = new GuestSyncResultDto("Success", 1, 2, 3, 6, DateTimeOffset.UtcNow);

        A.CallTo(() => syncHandler.Handle(A<SyncGuestsCommand>._, A<CancellationToken>._))
         .Returns(Task.FromResult<GuestSyncResultDto?>(syncResult));

        var result = await controller.Sync("event-1", CancellationToken.None);

        var ok = result.Result
                       .Should()
                       .BeOfType<OkObjectResult>()
                       .Subject;
        ok.Value
          .Should()
          .Be(syncResult);
        A.CallTo(() => syncHandler.Handle(A<SyncGuestsCommand>.That.Matches(command => command.EventId == "event-1"), A<CancellationToken>._))
         .MustHaveHappenedOnceExactly();
    }
}