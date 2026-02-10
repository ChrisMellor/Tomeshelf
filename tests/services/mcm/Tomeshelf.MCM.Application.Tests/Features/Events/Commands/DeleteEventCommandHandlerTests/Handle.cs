using Bogus;
using FakeItEasy;
using Shouldly;
using Tomeshelf.MCM.Application.Features.Events.Commands;
using Tomeshelf.MCM.Application.Services;

namespace Tomeshelf.MCM.Application.Tests.Features.Events.Commands.DeleteEventCommandHandlerTests;

public class Handle
{
    /// <summary>
    ///     Calls the service and returns result.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task CallsServiceAndReturnsResult()
    {
        // Arrange
        var faker = new Faker();
        var service = A.Fake<IEventService>();
        var handler = new DeleteEventCommandHandler(service);
        var eventId = faker.Random.AlphaNumeric(8);
        var command = new DeleteEventCommand(eventId);

        A.CallTo(() => service.DeleteAsync(eventId, A<CancellationToken>._))
         .Returns(Task.FromResult(true));

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBeTrue();
        A.CallTo(() => service.DeleteAsync(eventId, A<CancellationToken>._))
         .MustHaveHappenedOnceExactly();
    }
}