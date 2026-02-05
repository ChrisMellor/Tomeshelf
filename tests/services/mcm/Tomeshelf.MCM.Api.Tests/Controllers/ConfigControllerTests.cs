using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Tomeshelf.Application.Shared.Abstractions.Messaging;
using Tomeshelf.MCM.Api.Controllers;
using Tomeshelf.MCM.Application.Features.Events.Commands;
using Tomeshelf.MCM.Application.Features.Events.Queries;
using Tomeshelf.MCM.Application.Models;

namespace Tomeshelf.MCM.Api.Tests.Controllers;

public class ConfigControllerTests
{
    [Fact]
    public async Task Delete_ReturnsNoContent_WhenDeleted()
    {
        var getHandler = A.Fake<IQueryHandler<GetEventsQuery, IReadOnlyList<EventConfigModel>>>();
        var upsertHandler = A.Fake<ICommandHandler<UpsertEventCommand, bool>>();
        var deleteHandler = A.Fake<ICommandHandler<DeleteEventCommand, bool>>();
        var controller = new ConfigController(getHandler, upsertHandler, deleteHandler);
        var eventId = "event-1";

        A.CallTo(() => deleteHandler.Handle(A<DeleteEventCommand>._, A<CancellationToken>._))
         .Returns(Task.FromResult(true));

        var result = await controller.Delete(eventId, CancellationToken.None);

        result.Should()
              .BeOfType<NoContentResult>();
        A.CallTo(() => deleteHandler.Handle(A<DeleteEventCommand>.That.Matches(command => command.EventId == eventId), A<CancellationToken>._))
         .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Delete_ReturnsNotFound_WhenMissing()
    {
        var getHandler = A.Fake<IQueryHandler<GetEventsQuery, IReadOnlyList<EventConfigModel>>>();
        var upsertHandler = A.Fake<ICommandHandler<UpsertEventCommand, bool>>();
        var deleteHandler = A.Fake<ICommandHandler<DeleteEventCommand, bool>>();
        var controller = new ConfigController(getHandler, upsertHandler, deleteHandler);
        var eventId = "missing";

        A.CallTo(() => deleteHandler.Handle(A<DeleteEventCommand>._, A<CancellationToken>._))
         .Returns(Task.FromResult(false));

        var result = await controller.Delete(eventId, CancellationToken.None);

        var notFound = result.Should()
                             .BeOfType<NotFoundObjectResult>()
                             .Subject;
        notFound.Value
                .Should()
                .Be(eventId);
        A.CallTo(() => deleteHandler.Handle(A<DeleteEventCommand>.That.Matches(command => command.EventId == eventId), A<CancellationToken>._))
         .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Get_ReturnsOk_WithConfigs()
    {
        var getHandler = A.Fake<IQueryHandler<GetEventsQuery, IReadOnlyList<EventConfigModel>>>();
        var upsertHandler = A.Fake<ICommandHandler<UpsertEventCommand, bool>>();
        var deleteHandler = A.Fake<ICommandHandler<DeleteEventCommand, bool>>();
        var controller = new ConfigController(getHandler, upsertHandler, deleteHandler);
        var configs = new List<EventConfigModel>
        {
            new()
            {
                Id = "event-1",
                Name = "Event One"
            },
            new()
            {
                Id = "event-2",
                Name = "Event Two"
            }
        };

        A.CallTo(() => getHandler.Handle(A<GetEventsQuery>._, A<CancellationToken>._))
         .Returns(Task.FromResult<IReadOnlyList<EventConfigModel>>(configs));

        var result = await controller.Get(CancellationToken.None);

        var ok = result.Should()
                       .BeOfType<OkObjectResult>()
                       .Subject;
        var payload = ok.Value
                        .Should()
                        .BeAssignableTo<IReadOnlyList<EventConfigModel>>()
                        .Subject;
        payload.Should()
               .BeEquivalentTo(configs);

        A.CallTo(() => getHandler.Handle(A<GetEventsQuery>._, A<CancellationToken>._))
         .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Update_CallsUpsertHandler_AndReturnsOk()
    {
        var getHandler = A.Fake<IQueryHandler<GetEventsQuery, IReadOnlyList<EventConfigModel>>>();
        var upsertHandler = A.Fake<ICommandHandler<UpsertEventCommand, bool>>();
        var deleteHandler = A.Fake<ICommandHandler<DeleteEventCommand, bool>>();
        var controller = new ConfigController(getHandler, upsertHandler, deleteHandler);
        var model = new EventConfigModel
        {
            Id = "event-1",
            Name = "Event One"
        };

        A.CallTo(() => upsertHandler.Handle(A<UpsertEventCommand>._, A<CancellationToken>._))
         .Returns(Task.FromResult(true));

        var result = await controller.Update(model, CancellationToken.None);

        result.Should()
              .BeOfType<OkResult>();
        A.CallTo(() => upsertHandler.Handle(A<UpsertEventCommand>.That.Matches(command => command.Model == model), A<CancellationToken>._))
         .MustHaveHappenedOnceExactly();
    }
}