using Bogus;
using FakeItEasy;
using Shouldly;
using Tomeshelf.MCM.Application.Features.Events.Queries;
using Tomeshelf.MCM.Application.Models;
using Tomeshelf.MCM.Application.Services;

namespace Tomeshelf.MCM.Application.Tests.Features.Events.Queries.GetEventsQueryHandlerTests;

public class Handle
{
    [Fact]
    public async Task ValidQuery_CallsGetAllAsyncAndReturnsResult()
    {
        var faker = new Faker();
        var service = A.Fake<IEventService>();
        var handler = new GetEventsQueryHandler(service);
        IReadOnlyList<EventConfigModel> expectedEvents = new List<EventConfigModel>
        {
            new()
            {
                Id = faker.Random.AlphaNumeric(8),
                Name = faker.Company.CompanyName()
            },
            new()
            {
                Id = faker.Random.AlphaNumeric(8),
                Name = faker.Company.CompanyName()
            }
        };

        A.CallTo(() => service.GetAllAsync(A<CancellationToken>._))
         .Returns(Task.FromResult(expectedEvents));

        var result = await handler.Handle(new GetEventsQuery(), CancellationToken.None);

        result.Count.ShouldBe(expectedEvents.Count);
        for (var index = 0; index < expectedEvents.Count; index++)
        {
            result[index]
               .Id
               .ShouldBe(expectedEvents[index].Id);
            result[index]
               .Name
               .ShouldBe(expectedEvents[index].Name);
        }

        A.CallTo(() => service.GetAllAsync(A<CancellationToken>._))
         .MustHaveHappenedOnceExactly();
    }
}