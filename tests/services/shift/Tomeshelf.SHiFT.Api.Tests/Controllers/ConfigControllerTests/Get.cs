using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using Tomeshelf.Application.Shared.Abstractions.Messaging;
using Tomeshelf.SHiFT.Api.Controllers;
using Tomeshelf.SHiFT.Application.Features.Settings.Commands;
using Tomeshelf.SHiFT.Application.Features.Settings.Dtos;
using Tomeshelf.SHiFT.Application.Features.Settings.Queries;

namespace Tomeshelf.SHiFT.Api.Tests.Controllers.ConfigControllerTests;

public class Get
{
    [Fact]
    public async Task ReturnsNotFound_WhenMissing()
    {
        // Arrange
        var queryHandler = A.Fake<IQueryHandler<GetShiftSettingsQuery, ShiftSettingsDto?>>();
        var createHandler = A.Fake<ICommandHandler<CreateShiftSettingsCommand, int>>();
        var updateHandler = A.Fake<ICommandHandler<UpdateShiftSettingsCommand, bool>>();
        var deleteHandler = A.Fake<ICommandHandler<DeleteShiftSettingsCommand, bool>>();
        var controller = new ConfigController(queryHandler, createHandler, updateHandler, deleteHandler);

        A.CallTo(() => queryHandler.Handle(A<GetShiftSettingsQuery>._, A<CancellationToken>._))
         .Returns(Task.FromResult<ShiftSettingsDto?>(null));

        // Act
        var result = await controller.Get(3, CancellationToken.None);

        // Assert
        result.Result.ShouldBeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task ReturnsOk_WhenFound()
    {
        // Arrange
        var queryHandler = A.Fake<IQueryHandler<GetShiftSettingsQuery, ShiftSettingsDto?>>();
        var createHandler = A.Fake<ICommandHandler<CreateShiftSettingsCommand, int>>();
        var updateHandler = A.Fake<ICommandHandler<UpdateShiftSettingsCommand, bool>>();
        var deleteHandler = A.Fake<ICommandHandler<DeleteShiftSettingsCommand, bool>>();
        var controller = new ConfigController(queryHandler, createHandler, updateHandler, deleteHandler);
        var dto = new ShiftSettingsDto(7, "user@example.com", "psn", true, DateTimeOffset.UtcNow);

        A.CallTo(() => queryHandler.Handle(A<GetShiftSettingsQuery>._, A<CancellationToken>._))
         .Returns(Task.FromResult<ShiftSettingsDto?>(dto));

        // Act
        var result = await controller.Get(7, CancellationToken.None);

        // Assert
        var ok = result.Result.ShouldBeOfType<OkObjectResult>();
        ok.Value.ShouldBeSameAs(dto);
    }
}
