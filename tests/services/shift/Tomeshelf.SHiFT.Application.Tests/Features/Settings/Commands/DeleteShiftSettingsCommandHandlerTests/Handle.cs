using FakeItEasy;
using Shouldly;
using Tomeshelf.SHiFT.Application.Abstractions.Persistence;
using Tomeshelf.SHiFT.Application.Features.Settings.Commands;

namespace Tomeshelf.SHiFT.Application.Tests.Features.Settings.Commands.DeleteShiftSettingsCommandHandlerTests;

public class Handle
{
    /// <summary>
    ///     Deletes the and returns true.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task DeletesAndReturnsTrue()
    {
        // Arrange
        var repository = A.Fake<IShiftSettingsRepository>();
        var handler = new DeleteShiftSettingsCommandHandler(repository);
        var command = new DeleteShiftSettingsCommand(5);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBeTrue();
        A.CallTo(() => repository.DeleteAsync(5, A<CancellationToken>._))
         .MustHaveHappenedOnceExactly();
    }
}