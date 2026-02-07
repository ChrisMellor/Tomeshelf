using Microsoft.EntityFrameworkCore;
using Shouldly;
using Tomeshelf.MCM.Domain.Mcm;
using Tomeshelf.MCM.Infrastructure.Repositories;

namespace Tomeshelf.MCM.Infrastructure.Tests.GuestsRepositoryTests;

public class GetEventWithGuestsAsync
{
    [Fact]
    public async Task ReturnsEventWithGuests()
    {
        using var context = CreateContext();
        var repository = new GuestsRepository(context);
        var eventId = "test-event";
        var eventEntity = new EventEntity
        {
            Id = eventId,
            Name = "Test Event",
            Guests = new List<GuestEntity>
            {
                new GuestEntity
                {
                    Id = Guid.NewGuid(),
                    EventId = eventId,
                    Information = new GuestInfoEntity()
                },
                new GuestEntity
                {
                    Id = Guid.NewGuid(),
                    EventId = eventId,
                    Information = new GuestInfoEntity()
                }
            }
        };
        context.Events.Add(eventEntity);
        await context.SaveChangesAsync();

        var result = await repository.GetEventWithGuestsAsync(eventId, CancellationToken.None);

        result.ShouldNotBeNull();
        result!.Id.ShouldBe(eventId);
        result.Guests.Count.ShouldBe(2);
    }

    private static TomeshelfMcmDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TomeshelfMcmDbContext>().UseInMemoryDatabase(Guid.NewGuid()
                                                                                                   .ToString())
                                                                          .Options;

        return new TomeshelfMcmDbContext(options);
    }
}