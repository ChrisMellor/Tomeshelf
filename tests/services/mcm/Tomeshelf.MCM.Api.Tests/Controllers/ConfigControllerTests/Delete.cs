using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using Tomeshelf.Application.Shared.Abstractions.Messaging;
using Tomeshelf.MCM.Api.Controllers;
using Tomeshelf.MCM.Application.Features.Events.Commands;
using Tomeshelf.MCM.Application.Features.Events.Queries;
using Tomeshelf.MCM.Application.Models;

namespace Tomeshelf.MCM.Api.Tests.Controllers.ConfigControllerTests;

public class Delete
{
    [Fact]
    public async Task ReturnsNoContent_WhenDeleted()
    {
        // Arrange
        var getHandler = A.Fake<IQueryHandler<GetEventsQuery, IReadOnlyList<EventConfigModel>>>();
        var upsertHandler = A.Fake<ICommandHandler<UpsertEventCommand, bool>>();
        var deleteHandler = A.Fake<ICommandHandler<DeleteEventCommand, bool>>();
        var controller = new ConfigController(getHandler, upsertHandler, deleteHandler);
        var eventId = "event-1";

        A.CallTo(() => deleteHandler.Handle(A<DeleteEventCommand>._, A<CancellationToken>._))
         .Returns(Task.FromResult(true));

        // Act
        var result = await controller.Delete(eventId, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<NoContentResult>();
        A.CallTo(() => deleteHandler.Handle(A<DeleteEventCommand>.That.Matches(command => command.EventId == eventId), A<CancellationToken>._))
         .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ReturnsNotFound_WhenMissing()
    {
        // Arrange
        var getHandler = A.Fake<IQueryHandler<GetEventsQuery, IReadOnlyList<EventConfigModel>>>();
        var upsertHandler = A.Fake<ICommandHandler<UpsertEventCommand, bool>>();
        var deleteHandler = A.Fake<ICommandHandler<DeleteEventCommand, bool>>();
        var controller = new ConfigController(getHandler, upsertHandler, deleteHandler);
        var eventId = "missing";

        A.CallTo(() => deleteHandler.Handle(A<DeleteEventCommand>._, A<CancellationToken>._))
         .Returns(Task.FromResult(false));

        // Act
        var result = await controller.Delete(eventId, CancellationToken.None);

        // Assert
        var notFound = result.ShouldBeOfType<NotFoundObjectResult>();
        notFound.Value.ShouldBe(eventId);
        A.CallTo(() => deleteHandler.Handle(A<DeleteEventCommand>.That.Matches(command => command.EventId == eventId), A<CancellationToken>._))
         .MustHaveHappenedOnceExactly();
    }
}
