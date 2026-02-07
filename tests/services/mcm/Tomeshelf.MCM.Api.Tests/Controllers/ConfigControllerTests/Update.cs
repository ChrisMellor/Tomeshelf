using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using Tomeshelf.Application.Shared.Abstractions.Messaging;
using Tomeshelf.MCM.Api.Controllers;
using Tomeshelf.MCM.Application.Features.Events.Commands;
using Tomeshelf.MCM.Application.Features.Events.Queries;
using Tomeshelf.MCM.Application.Models;

namespace Tomeshelf.MCM.Api.Tests.Controllers.ConfigControllerTests;

public class Update
{
    [Fact]
    public async Task ReturnsOk_WhenUpsertSucceeds()
    {
        // Arrange
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

        // Act
        var result = await controller.Update(model, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<OkResult>();
        A.CallTo(() => upsertHandler.Handle(A<UpsertEventCommand>.That.Matches(command => command.Model == model), A<CancellationToken>._))
         .MustHaveHappenedOnceExactly();
    }
}
