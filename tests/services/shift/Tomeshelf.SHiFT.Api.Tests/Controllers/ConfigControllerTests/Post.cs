using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Shouldly;
using Tomeshelf.Application.Shared.Abstractions.Messaging;
using Tomeshelf.SHiFT.Api.Contracts;
using Tomeshelf.SHiFT.Api.Controllers;
using Tomeshelf.SHiFT.Application.Features.Settings.Commands;
using Tomeshelf.SHiFT.Application.Features.Settings.Dtos;
using Tomeshelf.SHiFT.Application.Features.Settings.Queries;

namespace Tomeshelf.SHiFT.Api.Tests.Controllers.ConfigControllerTests;

public class Post
{
    [Fact]
    public async Task ReturnsConflict_WhenDuplicateEmail()
    {
        // Arrange
        var queryHandler = A.Fake<IQueryHandler<GetShiftSettingsQuery, ShiftSettingsDto?>>();
        var listHandler = A.Fake<IQueryHandler<ListShiftSettingsQuery, IReadOnlyList<ShiftSettingsDto>>>();
        var createHandler = A.Fake<ICommandHandler<CreateShiftSettingsCommand, int>>();
        var updateHandler = A.Fake<ICommandHandler<UpdateShiftSettingsCommand, bool>>();
        var deleteHandler = A.Fake<ICommandHandler<DeleteShiftSettingsCommand, bool>>();
        var controller = new ConfigController(queryHandler, listHandler, createHandler, updateHandler, deleteHandler);
        var request = new ShiftSettingsUpdateRequest("user@example.com", "secret", "xbox");

        A.CallTo(() => createHandler.Handle(A<CreateShiftSettingsCommand>._, A<CancellationToken>._))
         .Throws<InvalidOperationException>();

        // Act
        var result = await controller.Post(request, CancellationToken.None);

        // Assert
        var conflict = result.ShouldBeOfType<ConflictObjectResult>();
        conflict.Value.ShouldBe("SHiFT email already exists.");
    }

    [Fact]
    public async Task ReturnsCreatedAtAction_WhenCreated()
    {
        // Arrange
        var queryHandler = A.Fake<IQueryHandler<GetShiftSettingsQuery, ShiftSettingsDto?>>();
        var listHandler = A.Fake<IQueryHandler<ListShiftSettingsQuery, IReadOnlyList<ShiftSettingsDto>>>();
        var createHandler = A.Fake<ICommandHandler<CreateShiftSettingsCommand, int>>();
        var updateHandler = A.Fake<ICommandHandler<UpdateShiftSettingsCommand, bool>>();
        var deleteHandler = A.Fake<ICommandHandler<DeleteShiftSettingsCommand, bool>>();
        var controller = new ConfigController(queryHandler, listHandler, createHandler, updateHandler, deleteHandler);
        var request = new ShiftSettingsUpdateRequest("user@example.com", "secret", "xbox");

        CreateShiftSettingsCommand? captured = null;
        A.CallTo(() => createHandler.Handle(A<CreateShiftSettingsCommand>._, A<CancellationToken>._))
         .Invokes(call => captured = call.GetArgument<CreateShiftSettingsCommand>(0))
         .Returns(Task.FromResult(42));

        // Act
        var result = await controller.Post(request, CancellationToken.None);

        // Assert
        var created = result.ShouldBeOfType<CreatedAtActionResult>();
        created.ActionName.ShouldBe(nameof(ConfigController.Get));
        created.RouteValues.ShouldNotBeNull();
        created.RouteValues!.ContainsKey("id")
               .ShouldBeTrue();
        created.RouteValues["id"]
               .ShouldBe(42);
        captured.ShouldNotBeNull();
        captured!.Email.ShouldBe("user@example.com");
        captured.Password.ShouldBe("secret");
        captured.DefaultService.ShouldBe("xbox");
    }
}
