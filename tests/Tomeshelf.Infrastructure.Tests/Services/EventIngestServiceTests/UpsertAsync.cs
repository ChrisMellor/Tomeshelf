using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Tomeshelf.Application.Contracts;
using Tomeshelf.Infrastructure.Persistence;
using Tomeshelf.Infrastructure.Services;

namespace Tomeshelf.Infrastructure.Tests.Services.EventIngestServiceTests;

public class EventIngestServiceTests
{
    [Fact]
    public async Task UpsertAsync_InsertsNewEventAndPeople()
    {
        // Arrange
        var dbOptions = new DbContextOptionsBuilder<TomeshelfComicConDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        using var db = new TomeshelfComicConDbContext(dbOptions);
        var sut = new EventIngestService(db);

        var evt = new EventDto
        {
            EventId = Guid.NewGuid().ToString(),
            EventName = "My Event",
            EventSlug = "2025-london",
            People = new List<PersonDto>
            {
                new()
                {
                    Id = Guid.NewGuid().ToString(), FirstName = "Ada", LastName = "Lovelace", Images = [],
                    GlobalCategories = []
                }
            }
        };

        // Act
        var changes = await sut.UpsertAsync(evt);

        // Assert
        changes.Should().BeGreaterThan(0);
        var ct = TestContext.Current.CancellationToken;
        (await db.Events.CountAsync(ct)).Should().Be(1);
        (await db.People.CountAsync(ct)).Should().Be(1);
        (await db.EventAppearances.CountAsync(ct)).Should().Be(1);
    }

    [Fact]
    public async Task UpsertAsync_UpdatesExistingEvent()
    {
        // Arrange
        var dbOptions = new DbContextOptionsBuilder<TomeshelfComicConDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        using var db = new TomeshelfComicConDbContext(dbOptions);
        var sut = new EventIngestService(db);

        var evt = new EventDto
        {
            EventId = "E1",
            EventName = "Old",
            EventSlug = "old-slug",
            People = []
        };
        await sut.UpsertAsync(evt);

        var updated = evt with { EventName = "New Name", EventSlug = "new-slug" };

        // Act
        await sut.UpsertAsync(updated);

        // Assert
        var entity = await db.Events.SingleAsync(e => e.ExternalId == "E1", TestContext.Current.CancellationToken);
        entity.Name.Should().Be("New Name");
        entity.Slug.Should().Be("new-slug");
        entity.UpdatedUtc.Should().NotBeNull();
    }
}