using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using FluentAssertions;
using System;
using System.Linq;
using System.Threading.Tasks;
using Tomeshelf.Domain.Entities.ComicCon;
using Tomeshelf.Infrastructure.Persistence;
using Tomeshelf.Infrastructure.Queries;

namespace Tomeshelf.Infrastructure.Tests.Queries.GuestQueriesTests;

public class GuestQueriesGetGuestsByCityAsyncTests
{
    [Fact]
    public async Task GetGuestsByCityAsync_GroupsByDate()
    {
        // Arrange
        var dbOptions = new DbContextOptionsBuilder<TomeshelfDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        using var db = new TomeshelfDbContext(dbOptions);

        var ev = new Event { ExternalId = Guid.NewGuid().ToString(), Name = "Event", Slug = "2025-london" };
        db.Events.Add(ev);
        var p1 = new Person { ExternalId = "P1", FirstName = "Ada", LastName = "Lovelace" };
        var p2 = new Person { ExternalId = "P2", FirstName = "Grace", LastName = "Hopper" };
        db.People.AddRange(p1, p2);
        db.EventAppearances.AddRange(
            new EventAppearance { Event = ev, Person = p1 },
            new EventAppearance { Event = ev, Person = p2 }
        );
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var queries = new GuestQueries(db, NullLogger<GuestQueries>.Instance);

        // Act
        var groups = await queries.GetGuestsByCityAsync("London", TestContext.Current.CancellationToken);

        // Assert
        groups.Should().NotBeEmpty();
        groups.Sum(group => group.Items.Count).Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task GetGuestsByCityAsync_EmptyCity_ReturnsEmpty()
    {
        // Arrange
        var dbOptions = new DbContextOptionsBuilder<TomeshelfDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        using var db = new TomeshelfDbContext(dbOptions);
        var queries = new GuestQueries(db, NullLogger<GuestQueries>.Instance);

        // Act
        var groups = await queries.GetGuestsByCityAsync("   ", TestContext.Current.CancellationToken);

        // Assert
        groups.Should().BeEmpty();
    }
}
