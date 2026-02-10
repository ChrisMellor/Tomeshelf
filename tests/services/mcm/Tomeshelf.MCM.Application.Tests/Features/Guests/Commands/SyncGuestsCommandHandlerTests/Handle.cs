using Bogus;
using FakeItEasy;
using Shouldly;
using Tomeshelf.MCM.Application.Contracts;
using Tomeshelf.MCM.Application.Features.Guests.Commands;
using Tomeshelf.MCM.Application.Models;
using Tomeshelf.MCM.Application.Services;

namespace Tomeshelf.MCM.Application.Tests.Features.Guests.Commands.SyncGuestsCommandHandlerTests;

public class Handle
{
    /// <summary>
    ///     Calls the service with event model.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task CallsServiceWithEventModel()
    {
        // Arrange
        var faker = new Faker();
        var service = A.Fake<IGuestsService>();
        var handler = new SyncGuestsCommandHandler(service);
        var eventId = faker.Random.AlphaNumeric(8);
        var command = new SyncGuestsCommand(eventId);
        var expected = new GuestSyncResultDto("Succeeded", 1, 2, 0, 3, faker.Date.RecentOffset());

        A.CallTo(() => service.SyncAsync(A<EventConfigModel>.That.Matches(model => (model.Id == eventId) && (model.Name == string.Empty)), A<CancellationToken>._))
         .Returns(Task.FromResult(expected));

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBeSameAs(expected);
        A.CallTo(() => service.SyncAsync(A<EventConfigModel>.That.Matches(model => (model.Id == eventId) && (model.Name == string.Empty)), A<CancellationToken>._))
         .MustHaveHappenedOnceExactly();
    }
}