using Shouldly;
using Tomeshelf.MCM.Infrastructure.Responses;

namespace Tomeshelf.MCM.Infrastructure.Tests.McmEventResponseTests;

public class Properties
{
    [Fact]
    public void CanSetAndGetValues()
    {
        // Arrange
        var eventId = "event-id-1";
        var eventName = "Event Name";
        var eventSlug = "event-slug";
        var people = new[]
        {
            new McmEventResponse.Person
            {
                Id = "person-id-1",
                FirstName = "John"
            }
        };

        // Act
        var response = new McmEventResponse
        {
            EventId = eventId,
            EventName = eventName,
            EventSlug = eventSlug,
            People = people
        };

        // Assert
        response.EventId.ShouldBe(eventId);
        response.EventName.ShouldBe(eventName);
        response.EventSlug.ShouldBe(eventSlug);
        response.People.ShouldBeSameAs(people);
    }
}