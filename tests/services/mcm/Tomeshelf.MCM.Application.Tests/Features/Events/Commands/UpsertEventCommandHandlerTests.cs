using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.MCM.Application.Features.Events.Commands;
using Tomeshelf.MCM.Application.Models;
using Tomeshelf.MCM.Application.Services;
using Xunit;

namespace Tomeshelf.MCM.Application.Tests.Features.Events.Commands;

public class UpsertEventCommandHandlerTests
{
    private readonly Mock<IEventService> _mockEventService;
    private readonly UpsertEventCommandHandler _handler;

    public UpsertEventCommandHandlerTests()
    {
        _mockEventService = new Mock<IEventService>();
        _handler = new UpsertEventCommandHandler(_mockEventService.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_CallsUpsertAsyncAndReturnsTrue()
    {
        // Arrange
        var model = new EventConfigModel { Id = "event1", Name = "Event 1" };
        var command = new UpsertEventCommand(model);

        _mockEventService
            .Setup(s => s.UpsertAsync(model, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result);
        _mockEventService.Verify(s => s.UpsertAsync(model, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_EventServiceThrowsException_ExceptionIsPropagated()
    {
        // Arrange
        var model = new EventConfigModel { Id = "event1", Name = "Event 1" };
        var command = new UpsertEventCommand(model);
        var expectedException = new InvalidOperationException("Upsert failed.");

        _mockEventService
            .Setup(s => s.UpsertAsync(model, It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var thrownException = await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(command, CancellationToken.None));
        Assert.Equal(expectedException.Message, thrownException.Message);
        _mockEventService.Verify(s => s.UpsertAsync(model, It.IsAny<CancellationToken>()), Times.Once);
    }
}
