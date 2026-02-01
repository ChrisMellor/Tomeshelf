using Moq;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.MCM.Application.Features.Events.Commands;
using Tomeshelf.MCM.Application.Services;
using Xunit;

namespace Tomeshelf.MCM.Application.Tests.Features.Events.Commands;

public class DeleteEventCommandHandlerTests
{
    [Fact]
    public async Task Handle_CallsServiceAndReturnsResult()
    {
        // Arrange
        var service = new Mock<IEventService>();
        var handler = new DeleteEventCommandHandler(service.Object);
        var command = new DeleteEventCommand("event-1");

        service.Setup(s => s.DeleteAsync("event-1", It.IsAny<CancellationToken>()))
               .ReturnsAsync(true);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result);
        service.Verify(s => s.DeleteAsync("event-1", It.IsAny<CancellationToken>()), Times.Once);
    }
}
