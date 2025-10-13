using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tomeshelf.Application.Contracts;
using Tomeshelf.Infrastructure.Persistence;
using Tomeshelf.Infrastructure.Services;

namespace Tomeshelf.Infrastructure.Tests.Services.EventIngestServiceTests;

public class EventIngestServiceCategoriesAndImagesDeltaTests
{
    [Fact]
    public async Task UpsertAsync_ReplacesImages_AndSynchronizesCategories()
    {
        // Arrange
        var dbOptions = new DbContextOptionsBuilder<TomeshelfComicConDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        using var db = new TomeshelfComicConDbContext(dbOptions);
        var sut = new EventIngestService(db);

        var evt1 = new EventDto
        {
            EventId = "E1",
            EventName = "Event",
            EventSlug = "2025-london",
            People = new List<PersonDto>
            {
                new PersonDto
                {
                    Id = "P1",
                    FirstName = "Ada",
                    LastName = "Lovelace",
                    Images = new List<ImageSetDto>{ new() { Big = "b1", Med = "m1", Small = "s1", Thumb = "t1" } },
                    GlobalCategories = new List<CategoryDto>{ new(){ Id = "A", Name = "Alpha" }, new(){ Id = "B", Name = "Beta" } }
                }
            }
        };

        // Act
        await sut.UpsertAsync(evt1);

        var evt2 = new EventDto
        {
            EventId = "E1",
            EventName = "Event",
            EventSlug = "2025-london",
            People = new List<PersonDto>
            {
                new PersonDto
                {
                    Id = "P1",
                    FirstName = "Ada",
                    LastName = "Lovelace",
                    Images = new List<ImageSetDto>{ new() { Big = "b2", Med = "m2", Small = "s2", Thumb = "t2" } },
                    GlobalCategories = new List<CategoryDto>{ new(){ Id = "B", Name = "Beta" }, new(){ Id = "C", Name = "Gamma" } }
                }
            }
        };

        await sut.UpsertAsync(evt2);

        // Assert
        var person = await db.People.Include(p => p.Images).Include(p => p.Categories).ThenInclude(pc => pc.Category)
            .SingleAsync(p => p.ExternalId == "P1", TestContext.Current.CancellationToken);
        person.Images.Should().HaveCount(1);
        person.Images.Single().Big.Should().Be("b2");
        var catIds = person.Categories.Select(c => c.Category.ExternalId).OrderBy(x => x).ToArray();
        catIds.Should().BeEquivalentTo(new[] { "B", "C" });
    }

    [Fact]
    public async Task UpsertAsync_AddsAndUpdatesSchedules_AndVenueLocations()
    {
        // Arrange
        var dbOptions = new DbContextOptionsBuilder<TomeshelfComicConDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        using var db = new TomeshelfComicConDbContext(dbOptions);
        var sut = new EventIngestService(db);

        var evt1 = new EventDto
        {
            EventId = "E2",
            EventName = "Event",
            EventSlug = "2025-london",
            People = new List<PersonDto>
            {
                new PersonDto
                {
                    Id = "P1",
                    FirstName = "Ada",
                    LastName = "Lovelace",
                    Schedules = new List<ScheduleDto>{ new(){ Id = "S1", Title = "Talk", Description = "Desc", StartTime = "2025-01-01T10:00:00Z", EndTime = "2025-01-01T11:00:00Z", NoEndTime = false, Location = "Room A", VenueLocation = new(){ Id = "V1", Name = "Hall" } } }
                }
            }
        };

        // Act
        await sut.UpsertAsync(evt1);

        var evt2 = new EventDto
        {
            EventId = "E2",
            EventName = "Event",
            EventSlug = "2025-london",
            People = new List<PersonDto>
            {
                new PersonDto
                {
                    Id = "P1",
                    FirstName = "Ada",
                    LastName = "Lovelace",
                    Schedules = new List<ScheduleDto>{
                        new(){ Id = "S1", Title = "Talk Updated", Description = "D2", StartTime = "2025-01-01T10:00:00Z", EndTime = "2025-01-01T11:00:00Z", NoEndTime = false, Location = "Room A", VenueLocation = new(){ Id = "V1", Name = "Hall Updated" } },
                        new(){ Id = "S2", Title = "Panel", Description = "P", StartTime = "2025-01-01T12:00:00Z", NoEndTime = true, Location = "Room B" }
                    }
                }
            }
        };

        await sut.UpsertAsync(evt2);

        // Assert
        var ea = await db.EventAppearances.Include(e => e.Schedules).ThenInclude(s => s.VenueLocation)
            .SingleAsync(a => a.Person.ExternalId == "P1" && a.Event.Slug == "2025-london", TestContext.Current.CancellationToken);
        ea.Schedules.Should().HaveCount(2);
        ea.Schedules.Single(s => s.ExternalId == "S1").Title.Should().Be("Talk Updated");
        ea.Schedules.Single(s => s.ExternalId == "S1").VenueLocation.Name.Should().Be("Hall Updated");
        ea.Schedules.Single(s => s.ExternalId == "S2").VenueLocation.Should().BeNull();
    }
}
