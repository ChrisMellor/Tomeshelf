using Moq;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.SHiFT.Application.Abstractions.Persistence;
using Tomeshelf.SHiFT.Application.Features.Settings.Commands;
using Xunit;

namespace Tomeshelf.SHiFT.Application.Tests.Features.Settings.Commands;

public class DeleteShiftSettingsCommandHandlerTests
{
    [Fact]
    public async Task Handle_DeletesAndReturnsTrue()
    {
        // Arrange
        var repository = new Mock<IShiftSettingsRepository>();
        var handler = new DeleteShiftSettingsCommandHandler(repository.Object);
        var command = new DeleteShiftSettingsCommand(5);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result);
        repository.Verify(r => r.DeleteAsync(5, It.IsAny<CancellationToken>()), Times.Once);
    }
}
