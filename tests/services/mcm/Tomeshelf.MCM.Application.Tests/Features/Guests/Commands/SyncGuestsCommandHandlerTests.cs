using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.MCM.Application.Contracts;
using Tomeshelf.MCM.Application.Features.Guests.Commands;
using Tomeshelf.MCM.Application.Models;
using Tomeshelf.MCM.Application.Services;
using Xunit;

namespace Tomeshelf.MCM.Application.Tests.Features.Guests.Commands;

public class SyncGuestsCommandHandlerTests
{
    [Fact]
    public async Task Handle_CallsServiceWithEventModel()
    {
        // Arrange
        var service = new Mock<IGuestsService>();
        var handler = new SyncGuestsCommandHandler(service.Object);
        var command = new SyncGuestsCommand("event-3");
        var expected = new GuestSyncResultDto("Succeeded", 1, 2, 0, 3, DateTimeOffset.UtcNow);

        service.Setup(s => s.SyncAsync(It.IsAny<EventConfigModel>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(expected);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Same(expected, result);
        service.Verify(s => s.SyncAsync(It.Is<EventConfigModel>(model => model.Id == "event-3" && model.Name == string.Empty), It.IsAny<CancellationToken>()), Times.Once);
    }
}
