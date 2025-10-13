using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Tomeshelf.Application.Contracts;
using Tomeshelf.Infrastructure.Persistence;
using Tomeshelf.Infrastructure.Services;

namespace Tomeshelf.Infrastructure.Tests.Services.EventIngestServiceTests;

public class CancellationCategorySetsRemovedTests
{
    [Fact]
    public async Task UpsertAsync_CanceledCategory_MarksNotVisible_AndSetsRemovedUtc()
    {
        // Arrange
        var dbOptions = new DbContextOptionsBuilder<TomeshelfComicConDbContext>().UseInMemoryDatabase(Guid.NewGuid()
                                                                                                          .ToString())
                                                                                 .Options;
        using var db = new TomeshelfComicConDbContext(dbOptions);
        var sut = new EventIngestService(db);

        var evt = new EventDto
        {
                EventId = "E-C1",
                EventName = "Event",
                EventSlug = "2025-london",
                People = new List<PersonDto>
                {
                        new()
                        {
                                Id = "P-C1",
                                Uid = "U1",
                                PubliclyVisible = true,
                                FirstName = "Test",
                                LastName = "Person",
                                GlobalCategories = new List<CategoryDto> { new() { Id = "X", Name = "Canceled" } }
                        }
                }
        };

        // Act
        await sut.UpsertAsync(evt);

        // Assert
        var person = await db.People.SingleAsync(p => p.ExternalId == "P-C1", TestContext.Current.CancellationToken);
        person.PubliclyVisible.Should()
              .BeFalse();
        person.RemovedUtc.Should()
              .NotBeNull();
    }

    [Fact]
    public async Task UpsertAsync_RemovedCanceledCategory_MakesVisible_AndClearsRemovedUtc()
    {
        // Arrange
        var dbOptions = new DbContextOptionsBuilder<TomeshelfComicConDbContext>().UseInMemoryDatabase(Guid.NewGuid()
                                                                                                          .ToString())
                                                                                 .Options;
        using var db = new TomeshelfComicConDbContext(dbOptions);
        var sut = new EventIngestService(db);

        var evt1 = new EventDto
        {
                EventId = "E-C2",
                EventName = "Event",
                EventSlug = "2025-london",
                People = new List<PersonDto>
                {
                        new()
                        {
                                Id = "P-C2",
                                Uid = "U2",
                                PubliclyVisible = true,
                                FirstName = "Test",
                                LastName = "Person",
                                GlobalCategories = new List<CategoryDto> { new() { Id = "X", Name = "Canceled" } }
                        }
                }
        };
        await sut.UpsertAsync(evt1);

        var evt2 = new EventDto
        {
                EventId = "E-C2",
                EventName = "Event",
                EventSlug = "2025-london",
                People = new List<PersonDto>
                {
                        new()
                        {
                                Id = "P-C2",
                                Uid = "U2",
                                PubliclyVisible = true,
                                FirstName = "Test",
                                LastName = "Person",
                                GlobalCategories = new List<CategoryDto>()
                        }
                }
        };

        // Act
        await sut.UpsertAsync(evt2);

        // Assert
        var person = await db.People.SingleAsync(p => p.ExternalId == "P-C2", TestContext.Current.CancellationToken);
        person.PubliclyVisible.Should()
              .BeTrue();
        person.RemovedUtc.Should()
              .BeNull();
    }
}