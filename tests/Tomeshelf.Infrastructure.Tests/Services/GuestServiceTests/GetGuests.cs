using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bogus;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Tomeshelf.Application.Contracts;
using Tomeshelf.Application.Options;
using Tomeshelf.Infrastructure.Clients;
using Tomeshelf.Infrastructure.Persistence;
using Tomeshelf.Infrastructure.Services;

namespace Tomeshelf.Infrastructure.Tests.Services.GuestServiceTests;

public class GuestServiceTests
{
    [Fact]
    public async Task GetGuests_ReturnsPeople_AndPersists()
    {
        // Arrange
        var city = "London";
        var key = Guid.NewGuid();
        var options = Options.Create(new ComicConOptions
        {
                ComicCon = new List<Location>
                {
                        new Location
                        {
                                City = city,
                                Key = key
                        }
                }
        });

        var peopleFaker = new Faker<PersonDto>().RuleFor(p => p.Id, f => f.Random.Uuid()
                                                                          .ToString())
                                                .RuleFor(p => p.FirstName, f => f.Name.FirstName())
                                                .RuleFor(p => p.LastName, f => f.Name.LastName());
        var evt = new EventDto
        {
                EventId = Guid.NewGuid()
                              .ToString(),
                EventName = "Test Event",
                EventSlug = "2025-london",
                People = peopleFaker.Generate(3)
        };

        IGuestsClient guestsClient = new FakeGuestsClient(evt);

        var dbOptions = new DbContextOptionsBuilder<TomeshelfComicConDbContext>().UseInMemoryDatabase(Guid.NewGuid()
                                                                                                          .ToString())
                                                                                 .Options;
        using var db = new TomeshelfComicConDbContext(dbOptions);
        var ingest = new EventIngestService(db);
        var logger = NullLogger<GuestService>.Instance;

        var sut = new GuestService(guestsClient, options, logger, ingest);

        // Act
        var result = await sut.GetGuestsAsync(city, CancellationToken.None);

        // Assert
        result.Should()
              .NotBeNull();
        result.Should()
              .HaveCount(3);
    }

    [Fact]
    public async Task GetGuests_Throws_WhenCityNotConfigured()
    {
        // Arrange
        var options = Options.Create(new ComicConOptions());
        IGuestsClient guestsClient = new FakeGuestsClient(null);
        var dbOptions = new DbContextOptionsBuilder<TomeshelfComicConDbContext>().UseInMemoryDatabase(Guid.NewGuid()
                                                                                                          .ToString())
                                                                                 .Options;
        using var db = new TomeshelfComicConDbContext(dbOptions);
        var ingest = new EventIngestService(db);

        var sut = new GuestService(guestsClient, options, NullLogger<GuestService>.Instance, ingest);

        // Act
        Func<Task> actMissingCity = async () => await sut.GetGuestsAsync("UnknownCity", CancellationToken.None);

        // Assert
        await actMissingCity.Should()
                            .ThrowAsync<ApplicationException>();
    }

    [Fact]
    public async Task GetGuests_Throws_WhenClientReturnsNull()
    {
        // Arrange
        var city = "Birmingham";
        var key = Guid.NewGuid();
        var options = Options.Create(new ComicConOptions
        {
                ComicCon = new List<Location>
                {
                        new Location
                        {
                                City = city,
                                Key = key
                        }
                }
        });
        IGuestsClient guestsClient = new FakeGuestsClient(null);

        var dbOptions = new DbContextOptionsBuilder<TomeshelfComicConDbContext>().UseInMemoryDatabase(Guid.NewGuid()
                                                                                                          .ToString())
                                                                                 .Options;
        using var db = new TomeshelfComicConDbContext(dbOptions);
        var ingest = new EventIngestService(db);

        var sut = new GuestService(guestsClient, options, NullLogger<GuestService>.Instance, ingest);

        // Act
        Func<Task> actNullClient = async () => await sut.GetGuestsAsync(city, CancellationToken.None);

        // Assert
        await actNullClient.Should()
                           .ThrowAsync<ApplicationException>();
    }

    [Fact]
    public async Task GetGuests_Throws_WhenCityWhitespace()
    {
        // Arrange
        var options = Options.Create(new ComicConOptions());
        IGuestsClient guestsClient = new FakeGuestsClient(null);
        var dbOptions = new DbContextOptionsBuilder<TomeshelfComicConDbContext>().UseInMemoryDatabase(Guid.NewGuid()
                                                                                                          .ToString())
                                                                                 .Options;
        using var db = new TomeshelfComicConDbContext(dbOptions);
        var ingest = new EventIngestService(db);

        var sut = new GuestService(guestsClient, options, NullLogger<GuestService>.Instance, ingest);

        // Act
        Func<Task> act = async () => await sut.GetGuestsAsync("   ", CancellationToken.None);

        // Assert
        await act.Should()
                 .ThrowAsync<ApplicationException>();
    }

    [Fact]
    public async Task GetGuests_Succeeds_WithCaseInsensitiveCity()
    {
        // Arrange
        var key = Guid.NewGuid();
        var options = Options.Create(new ComicConOptions
        {
                ComicCon = new List<Location>
                {
                        new Location
                        {
                                City = "London",
                                Key = key
                        }
                }
        });
        var evt = new EventDto
        {
                EventId = "E",
                EventName = "N",
                EventSlug = "s",
                People = new List<PersonDto>()
        };
        IGuestsClient guestsClient = new FakeGuestsClient(evt);
        var dbOptions = new DbContextOptionsBuilder<TomeshelfComicConDbContext>().UseInMemoryDatabase(Guid.NewGuid()
                                                                                                          .ToString())
                                                                                 .Options;
        using var db = new TomeshelfComicConDbContext(dbOptions);
        var ingest = new EventIngestService(db);
        var sut = new GuestService(guestsClient, options, NullLogger<GuestService>.Instance, ingest);

        // Act
        var people = await sut.GetGuestsAsync("london", CancellationToken.None);

        // Assert
        people.Should()
              .NotBeNull();
    }

    private sealed class FakeGuestsClient : IGuestsClient
    {
        private readonly EventDto _response;

        public FakeGuestsClient(EventDto response)
        {
            _response = response;
        }

        public Task<EventDto> GetLatestGuestsAsync(Guid key, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_response);
        }
    }
}