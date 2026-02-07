using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using Shouldly;
using Tomeshelf.Application.Shared.Abstractions.Messaging;
using Tomeshelf.MCM.Api.Controllers;
using Tomeshelf.MCM.Application.Features.Events.Commands;
using Tomeshelf.MCM.Application.Features.Events.Queries;
using Tomeshelf.MCM.Application.Models;

namespace Tomeshelf.MCM.Api.Tests.Controllers.ConfigControllerTests;

public class Get
{
    [Fact]
    public async Task ReturnsOk_WithConfigs()
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

        var ok = result.ShouldBeOfType<OkObjectResult>();
        var payload = ok.Value.ShouldBeAssignableTo<IReadOnlyList<EventConfigModel>>();
        payload.ShouldBe(configs);
        A.CallTo(() => getHandler.Handle(A<GetEventsQuery>._, A<CancellationToken>._))
         .MustHaveHappenedOnceExactly();
    }
}