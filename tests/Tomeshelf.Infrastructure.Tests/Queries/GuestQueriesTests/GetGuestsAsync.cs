using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Tomeshelf.Domain.Entities.ComicCon;
using Tomeshelf.Infrastructure.Persistence;
using Tomeshelf.Infrastructure.Queries;

namespace Tomeshelf.Infrastructure.Tests.Queries.GuestQueriesTests;

public class GuestQueriesGetGuestsAsyncTests
{
    [Fact]
    public async Task GetGuestsAsync_SearchFiltersResults()
    {
        // Arrange
        var dbOptions = new DbContextOptionsBuilder<TomeshelfComicConDbContext>().UseInMemoryDatabase(Guid.NewGuid()
                                                                                                          .ToString())
                                                                                 .Options;
        using var db = new TomeshelfComicConDbContext(dbOptions);
        var ev = new Event
        {
                ExternalId = "E1",
                Name = "Event",
                Slug = "2025-london"
        };
        db.Events.Add(ev);
        var ada = new Person
        {
                ExternalId = "P1",
                FirstName = "Ada",
                LastName = "Lovelace",
                KnownFor = "Math"
        };
        var grace = new Person
        {
                ExternalId = "P2",
                FirstName = "Grace",
                LastName = "Hopper",
                KnownFor = "COBOL"
        };
        db.People.AddRange(ada, grace);
        db.EventAppearances.AddRange(new EventAppearance
        {
                Event = ev,
                Person = ada
        }, new EventAppearance
        {
                Event = ev,
                Person = grace
        });
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var queries = new GuestQueries(db, NullLogger<GuestQueries>.Instance);

        // Act
        var (filteredItems, total) = await queries.GetGuestsAsync("2025-london", null, "Ada", 1, 10, TestContext.Current.CancellationToken);

        // Assert
        total.Should()
             .Be(1);
        filteredItems.Should()
                     .ContainSingle(i => i.FirstName == "Ada");
    }
}