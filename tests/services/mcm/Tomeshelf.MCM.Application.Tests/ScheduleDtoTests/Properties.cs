using Shouldly;
using Tomeshelf.MCM.Application.Mcm;

namespace Tomeshelf.MCM.Application.Tests.ScheduleDtoTests;

public class Properties
{
    [Fact]
    public void CanSetAndGetValues()
    {
        var id = "schedule-id-1";
        var title = "Panel Discussion";
        var description = "A panel discussion on various topics.";
        var startTime = "10:00 AM";
        var endTime = "11:00 AM";
        var noEndTime = false;
        var location = "Main Hall";
        var venueLocation = new VenueLocationDto
        {
            Id = "venue-id-1",
            Name = "Convention Center"
        };

        var dto = new ScheduleDto
        {
            Id = id,
            Title = title,
            Description = description,
            StartTime = startTime,
            EndTime = endTime,
            NoEndTime = noEndTime,
            Location = location,
            VenueLocation = venueLocation
        };

        dto.Id.ShouldBe(id);
        dto.Title.ShouldBe(title);
        dto.Description.ShouldBe(description);
        dto.StartTime.ShouldBe(startTime);
        dto.EndTime.ShouldBe(endTime);
        dto.NoEndTime.ShouldBe(noEndTime);
        dto.Location.ShouldBe(location);
        dto.VenueLocation.ShouldBeSameAs(venueLocation);
    }

    [Fact]
    public void DefaultsAreExpected()
    {
        var dto = new ScheduleDto();

        var id = dto.Id;
        var title = dto.Title;
        var description = dto.Description;
        var startTime = dto.StartTime;
        var endTime = dto.EndTime;
        var location = dto.Location;
        var venueLocation = dto.VenueLocation;

        id.ShouldBeNull();
        title.ShouldBeNull();
        description.ShouldBeNull();
        startTime.ShouldBeNull();
        endTime.ShouldBeNull();
        dto.NoEndTime.ShouldBeFalse();
        location.ShouldBeNull();
        venueLocation.ShouldBeNull();
    }
}