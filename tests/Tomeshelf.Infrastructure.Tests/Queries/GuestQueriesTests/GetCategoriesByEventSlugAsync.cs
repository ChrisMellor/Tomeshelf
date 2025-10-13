using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Tomeshelf.Domain.Entities.ComicCon;
using Tomeshelf.Infrastructure.Persistence;
using Tomeshelf.Infrastructure.Queries;

namespace Tomeshelf.Infrastructure.Tests.Queries.GuestQueriesTests;

public class GuestQueriesGetCategoriesByEventSlugAsyncTests
{
    [Fact]
    public async Task GetCategoriesByEventSlugAsync_ReturnsEmpty_WhenEventNotFound()
    {
        // Arrange
        var dbOptions = new DbContextOptionsBuilder<TomeshelfComicConDbContext>().UseInMemoryDatabase(Guid.NewGuid()
                                                                                                          .ToString())
                                                                                 .Options;
        using var db = new TomeshelfComicConDbContext(dbOptions);
        var queries = new GuestQueries(db, NullLogger<GuestQueries>.Instance);

        // Act
        var cats = await queries.GetCategoriesByEventSlugAsync("missing", TestContext.Current.CancellationToken);

        // Assert
        cats.Should()
            .BeEmpty();
    }

    [Fact]
    public async Task GetCategoriesByEventSlugAsync_ReturnsDistinctSorted()
    {
        // Arrange
        var dbOptions = new DbContextOptionsBuilder<TomeshelfComicConDbContext>().UseInMemoryDatabase(Guid.NewGuid()
                                                                                                          .ToString())
                                                                                 .Options;
        using var db = new TomeshelfComicConDbContext(dbOptions);

        var ev = new Event { ExternalId = "E1", Name = "Event", Slug = "2025-london" };
        var p = new Person { ExternalId = "P1", FirstName = "A", LastName = "B" };
        var c1 = new Category { ExternalId = "A", Name = "Alpha" };
        var c2 = new Category { ExternalId = "B", Name = "Beta" };
        db.Events.Add(ev);
        db.People.Add(p);
        db.Categories.AddRange(c1, c2);
        db.PersonCategories.AddRange(new PersonCategory { Person = p, Category = c1 }, new PersonCategory { Person = p, Category = c2 });
        db.EventAppearances.Add(new EventAppearance { Event = ev, Person = p });
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var queries = new GuestQueries(db, NullLogger<GuestQueries>.Instance);

        // Act
        var cats = await queries.GetCategoriesByEventSlugAsync("2025-london", TestContext.Current.CancellationToken);

        // Assert
        cats.Should()
            .HaveCount(2);
        cats.Should()
            .ContainEquivalentOf(("A", "Alpha"));
        cats.Should()
            .ContainEquivalentOf(("B", "Beta"));
    }
}