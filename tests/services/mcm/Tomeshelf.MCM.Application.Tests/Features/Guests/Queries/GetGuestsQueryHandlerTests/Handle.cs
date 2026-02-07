using Bogus;
using FakeItEasy;
using Shouldly;
using Tomeshelf.MCM.Application.Contracts;
using Tomeshelf.MCM.Application.Features.Guests.Queries;
using Tomeshelf.MCM.Application.Models;
using Tomeshelf.MCM.Application.Services;

namespace Tomeshelf.MCM.Application.Tests.Features.Guests.Queries.GetGuestsQueryHandlerTests;

public class Handle
{
    [Fact]
    public async Task BuildsEventConfigModelAndCallsService()
    {
        var faker = new Faker();
        var service = A.Fake<IGuestsService>();
        var handler = new GetGuestsQueryHandler(service);
        var eventId = faker.Random.AlphaNumeric(8);
        var eventName = faker.Company.CompanyName();
        var query = new GetGuestsQuery(eventId, 2, 25, eventName, true);
        var expected = new PagedResult<GuestDto>(0, new List<GuestDto>(), 2, 25);

        A.CallTo(() => service.GetAsync(A<EventConfigModel>.That.Matches(model => (model.Id == eventId) && (model.Name == eventName)), 2, 25, true, A<CancellationToken>._))
         .Returns(Task.FromResult(expected));

        var result = await handler.Handle(query, CancellationToken.None);

        result.ShouldBeSameAs(expected);
        A.CallTo(() => service.GetAsync(A<EventConfigModel>.That.Matches(model => (model.Id == eventId) && (model.Name == eventName)), 2, 25, true, A<CancellationToken>._))
         .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task UsesEmptyNameWhenMissing()
    {
        var faker = new Faker();
        var service = A.Fake<IGuestsService>();
        var handler = new GetGuestsQueryHandler(service);
        var eventId = faker.Random.AlphaNumeric(8);
        var query = new GetGuestsQuery(eventId, 0, 10, null, false);
        var expected = new PagedResult<GuestDto>(0, new List<GuestDto>(), 0, 10);

        A.CallTo(() => service.GetAsync(A<EventConfigModel>.That.Matches(model => (model.Id == eventId) && (model.Name == string.Empty)), 0, 10, false, A<CancellationToken>._))
         .Returns(Task.FromResult(expected));

        var result = await handler.Handle(query, CancellationToken.None);

        result.ShouldBeSameAs(expected);
        A.CallTo(() => service.GetAsync(A<EventConfigModel>.That.Matches(model => (model.Id == eventId) && (model.Name == string.Empty)), 0, 10, false, A<CancellationToken>._))
         .MustHaveHappenedOnceExactly();
    }
}