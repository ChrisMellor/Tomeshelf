using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using Shouldly;
using Tomeshelf.Application.Shared.Abstractions.Messaging;
using Tomeshelf.MCM.Api.Controllers;
using Tomeshelf.MCM.Application.Contracts;
using Tomeshelf.MCM.Application.Features.Guests.Commands;
using Tomeshelf.MCM.Application.Features.Guests.Queries;

namespace Tomeshelf.MCM.Api.Tests.Controllers.GuestsControllerTests;

public class Sync
{
    [Fact]
    public async Task ReturnsNotFound_WhenSyncResultIsNull()
    {
        var queryHandler = A.Fake<IQueryHandler<GetGuestsQuery, PagedResult<GuestDto>>>();
        var syncHandler = A.Fake<ICommandHandler<SyncGuestsCommand, GuestSyncResultDto?>>();
        var controller = new GuestsController(queryHandler, syncHandler);

        A.CallTo(() => syncHandler.Handle(A<SyncGuestsCommand>._, A<CancellationToken>._))
         .Returns(Task.FromResult<GuestSyncResultDto?>(null));

        var result = await controller.Sync("event-1", CancellationToken.None);

        var notFound = result.Result.ShouldBeOfType<NotFoundObjectResult>();
        notFound.Value.ShouldBe("event-1");
        A.CallTo(() => syncHandler.Handle(A<SyncGuestsCommand>.That.Matches(command => command.EventId == "event-1"), A<CancellationToken>._))
         .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ReturnsOk_WithSyncResult()
    {
        var queryHandler = A.Fake<IQueryHandler<GetGuestsQuery, PagedResult<GuestDto>>>();
        var syncHandler = A.Fake<ICommandHandler<SyncGuestsCommand, GuestSyncResultDto?>>();
        var controller = new GuestsController(queryHandler, syncHandler);
        var syncResult = new GuestSyncResultDto("Success", 1, 2, 3, 6, DateTimeOffset.UtcNow);

        A.CallTo(() => syncHandler.Handle(A<SyncGuestsCommand>._, A<CancellationToken>._))
         .Returns(Task.FromResult<GuestSyncResultDto?>(syncResult));

        var result = await controller.Sync("event-1", CancellationToken.None);

        var ok = result.Result.ShouldBeOfType<OkObjectResult>();
        ok.Value.ShouldBe(syncResult);
        A.CallTo(() => syncHandler.Handle(A<SyncGuestsCommand>.That.Matches(command => command.EventId == "event-1"), A<CancellationToken>._))
         .MustHaveHappenedOnceExactly();
    }
}