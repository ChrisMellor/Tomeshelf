using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using Shouldly;
using Tomeshelf.Application.Shared.Abstractions.Messaging;
using Tomeshelf.SHiFT.Api.Contracts;
using Tomeshelf.SHiFT.Api.Controllers;
using Tomeshelf.SHiFT.Application.Features.Settings.Commands;
using Tomeshelf.SHiFT.Application.Features.Settings.Dtos;
using Tomeshelf.SHiFT.Application.Features.Settings.Queries;

namespace Tomeshelf.SHiFT.Api.Tests.Controllers.ConfigControllerTests;

public class Put
{
    [Fact]
    public async Task ReturnsConflict_WhenDuplicateEmail()
    {
        var queryHandler = A.Fake<IQueryHandler<GetShiftSettingsQuery, ShiftSettingsDto?>>();
        var createHandler = A.Fake<ICommandHandler<CreateShiftSettingsCommand, int>>();
        var updateHandler = A.Fake<ICommandHandler<UpdateShiftSettingsCommand, bool>>();
        var deleteHandler = A.Fake<ICommandHandler<DeleteShiftSettingsCommand, bool>>();
        var controller = new ConfigController(queryHandler, createHandler, updateHandler, deleteHandler);
        var request = new ShiftSettingsUpdateRequest("user@example.com", "secret", "xbox");

        A.CallTo(() => updateHandler.Handle(A<UpdateShiftSettingsCommand>._, A<CancellationToken>._))
         .Throws<InvalidOperationException>();

        var result = await controller.Put(7, request, CancellationToken.None);

        var conflict = result.ShouldBeOfType<ConflictObjectResult>();
        conflict.Value.ShouldBe("SHiFT email already exists.");
    }

    [Fact]
    public async Task ReturnsNoContent_WhenUpdated()
    {
        var queryHandler = A.Fake<IQueryHandler<GetShiftSettingsQuery, ShiftSettingsDto?>>();
        var createHandler = A.Fake<ICommandHandler<CreateShiftSettingsCommand, int>>();
        var updateHandler = A.Fake<ICommandHandler<UpdateShiftSettingsCommand, bool>>();
        var deleteHandler = A.Fake<ICommandHandler<DeleteShiftSettingsCommand, bool>>();
        var controller = new ConfigController(queryHandler, createHandler, updateHandler, deleteHandler);
        var request = new ShiftSettingsUpdateRequest("user@example.com", "secret", "xbox");

        UpdateShiftSettingsCommand? captured = null;
        A.CallTo(() => updateHandler.Handle(A<UpdateShiftSettingsCommand>._, A<CancellationToken>._))
         .Invokes(call => captured = call.GetArgument<UpdateShiftSettingsCommand>(0))
         .Returns(Task.FromResult(true));

        var result = await controller.Put(7, request, CancellationToken.None);

        result.ShouldBeOfType<NoContentResult>();
        captured.ShouldNotBeNull();
        captured!.Id.ShouldBe(7);
        captured.Email.ShouldBe("user@example.com");
        captured.Password.ShouldBe("secret");
        captured.DefaultService.ShouldBe("xbox");
    }

    [Fact]
    public async Task ReturnsNotFound_WhenMissing()
    {
        var queryHandler = A.Fake<IQueryHandler<GetShiftSettingsQuery, ShiftSettingsDto?>>();
        var createHandler = A.Fake<ICommandHandler<CreateShiftSettingsCommand, int>>();
        var updateHandler = A.Fake<ICommandHandler<UpdateShiftSettingsCommand, bool>>();
        var deleteHandler = A.Fake<ICommandHandler<DeleteShiftSettingsCommand, bool>>();
        var controller = new ConfigController(queryHandler, createHandler, updateHandler, deleteHandler);
        var request = new ShiftSettingsUpdateRequest("user@example.com", "secret", "xbox");

        A.CallTo(() => updateHandler.Handle(A<UpdateShiftSettingsCommand>._, A<CancellationToken>._))
         .Returns(Task.FromResult(false));

        var result = await controller.Put(7, request, CancellationToken.None);

        result.ShouldBeOfType<NotFoundResult>();
    }
}