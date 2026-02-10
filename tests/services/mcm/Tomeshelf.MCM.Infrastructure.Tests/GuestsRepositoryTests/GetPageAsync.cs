using Microsoft.EntityFrameworkCore;
using Shouldly;
using Tomeshelf.MCM.Domain.Mcm;
using Tomeshelf.MCM.Infrastructure.Repositories;

namespace Tomeshelf.MCM.Infrastructure.Tests.GuestsRepositoryTests;

public class GetPageAsync
{
    /// <summary>
    ///     Returns paged guests excluding deleted.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task ReturnsPagedGuests_ExcludingDeleted()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new GuestsRepository(context);
        var eventId = "test-event";
        var guests = new List<GuestEntity>
        {
            new GuestEntity
            {
                Id = Guid.NewGuid(),
                EventId = eventId,
                Information = new GuestInfoEntity { FirstName = "A" }
            },
            new GuestEntity
            {
                Id = Guid.NewGuid(),
                EventId = eventId,
                Information = new GuestInfoEntity { FirstName = "B" }
            },
            new GuestEntity
            {
                Id = Guid.NewGuid(),
                EventId = eventId,
                Information = new GuestInfoEntity { FirstName = "C" },
                IsDeleted = true
            }
        };
        context.Guests.AddRange(guests);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetPageAsync(eventId, 1, 2, false, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Total.ShouldBe(2);
        result.Items.Count.ShouldBe(2);
        result.Items
              .First()
              .Name
              .Trim()
              .ShouldBe("A");
    }

    /// <summary>
    ///     Returns paged guests including deleted.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task ReturnsPagedGuests_IncludingDeleted()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new GuestsRepository(context);
        var eventId = "test-event";
        var guests = new List<GuestEntity>
        {
            new GuestEntity
            {
                Id = Guid.NewGuid(),
                EventId = eventId,
                Information = new GuestInfoEntity { FirstName = "A" }
            },
            new GuestEntity
            {
                Id = Guid.NewGuid(),
                EventId = eventId,
                Information = new GuestInfoEntity { FirstName = "B" }
            },
            new GuestEntity
            {
                Id = Guid.NewGuid(),
                EventId = eventId,
                Information = new GuestInfoEntity { FirstName = "C" },
                IsDeleted = true
            }
        };
        context.Guests.AddRange(guests);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetPageAsync(eventId, 1, 3, true, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Total.ShouldBe(3);
        result.Items.Count.ShouldBe(3);
    }

    /// <summary>
    ///     Creates the context.
    /// </summary>
    /// <returns>The result of the operation.</returns>
    private static TomeshelfMcmDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TomeshelfMcmDbContext>().UseInMemoryDatabase(Guid.NewGuid()
                                                                                                   .ToString())
                                                                          .Options;

        return new TomeshelfMcmDbContext(options);
    }
}