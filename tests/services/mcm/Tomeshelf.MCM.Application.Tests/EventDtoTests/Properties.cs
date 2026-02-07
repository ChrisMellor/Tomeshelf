using Shouldly;
using Tomeshelf.MCM.Application.Mcm;

namespace Tomeshelf.MCM.Application.Tests.EventDtoTests;

public class Properties
{
    [Fact]
    public void CanSetAndGetValues()
    {
        var eventId = "event-id-1";
        var eventName = "Event Name";
        var eventSlug = "event-slug";
        var people = new List<PersonDto>
        {
            new()
            {
                Id = "person-id-1",
                FirstName = "John"
            }
        };

        var dto = new EventDto
        {
            EventId = eventId,
            EventName = eventName,
            EventSlug = eventSlug,
            People = people
        };

        dto.EventId.ShouldBe(eventId);
        dto.EventName.ShouldBe(eventName);
        dto.EventSlug.ShouldBe(eventSlug);
        dto.People.ShouldBeSameAs(people);
    }

    [Fact]
    public void DefaultsAreEmptyOrNull()
    {
        var dto = new EventDto();

        var eventId = dto.EventId;
        var eventName = dto.EventName;
        var eventSlug = dto.EventSlug;
        var people = dto.People;

        eventId.ShouldBeNull();
        eventName.ShouldBeNull();
        eventSlug.ShouldBeNull();
        people.ShouldNotBeNull();
        people.ShouldBeEmpty();
    }
}