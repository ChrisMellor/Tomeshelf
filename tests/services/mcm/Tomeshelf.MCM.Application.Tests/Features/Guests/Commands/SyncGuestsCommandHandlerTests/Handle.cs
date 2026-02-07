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
    [Fact]
    public async Task CallsServiceWithEventModel()
    {
        var faker = new Faker();
        var service = A.Fake<IGuestsService>();
        var handler = new SyncGuestsCommandHandler(service);
        var eventId = faker.Random.AlphaNumeric(8);
        var command = new SyncGuestsCommand(eventId);
        var expected = new GuestSyncResultDto("Succeeded", 1, 2, 0, 3, faker.Date.RecentOffset());

        A.CallTo(() => service.SyncAsync(A<EventConfigModel>.That.Matches(model => (model.Id == eventId) && (model.Name == string.Empty)), A<CancellationToken>._))
         .Returns(Task.FromResult(expected));

        var result = await handler.Handle(command, CancellationToken.None);

        result.ShouldBeSameAs(expected);
        A.CallTo(() => service.SyncAsync(A<EventConfigModel>.That.Matches(model => (model.Id == eventId) && (model.Name == string.Empty)), A<CancellationToken>._))
         .MustHaveHappenedOnceExactly();
    }
}