using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using Shouldly;
using Tomeshelf.Application.Shared.Abstractions.Messaging;
using Tomeshelf.SHiFT.Api.Controllers;
using Tomeshelf.SHiFT.Application.Features.Settings.Commands;
using Tomeshelf.SHiFT.Application.Features.Settings.Dtos;
using Tomeshelf.SHiFT.Application.Features.Settings.Queries;

namespace Tomeshelf.SHiFT.Api.Tests.Controllers.ConfigControllerTests;

public class Delete
{
    [Fact]
    public async Task ReturnsNoContent()
    {
        var queryHandler = A.Fake<IQueryHandler<GetShiftSettingsQuery, ShiftSettingsDto?>>();
        var createHandler = A.Fake<ICommandHandler<CreateShiftSettingsCommand, int>>();
        var updateHandler = A.Fake<ICommandHandler<UpdateShiftSettingsCommand, bool>>();
        var deleteHandler = A.Fake<ICommandHandler<DeleteShiftSettingsCommand, bool>>();
        var controller = new ConfigController(queryHandler, createHandler, updateHandler, deleteHandler);

        var result = await controller.Delete(12, CancellationToken.None);

        result.ShouldBeOfType<NoContentResult>();
        A.CallTo(() => deleteHandler.Handle(A<DeleteShiftSettingsCommand>.That.Matches(command => command.Id == 12), A<CancellationToken>._))
         .MustHaveHappenedOnceExactly();
    }
}