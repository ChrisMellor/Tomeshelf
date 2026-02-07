using Microsoft.EntityFrameworkCore;
using Shouldly;
using Tomeshelf.MCM.Domain.Mcm;
using Tomeshelf.MCM.Infrastructure.Repositories;

namespace Tomeshelf.MCM.Infrastructure.Tests.EventRepositoryTests;

public class DeleteAsync
{
    [Fact]
    public async Task DeletesEventWithGuests_WhenFound()
    {
        using var context = CreateContext();
        var repository = new EventRepository(context);
        var eventId = "event-id-1";
        var guestInfoId = Guid.NewGuid();
        var guestId = Guid.NewGuid();
        var guest = new GuestEntity
        {
            Id = guestId,
            GuestInfoId = guestInfoId
        };
        var guestInfo = new GuestInfoEntity
        {
            Id = guestInfoId,
            FirstName = "John",
            LastName = "Doe",
            GuestId = guestId
        };
        var eventEntity = new EventEntity
        {
            Id = eventId,
            Name = "Event 1",
            Guests = new List<GuestEntity> { guest }
        };

        context.Information.Add(guestInfo);
        context.Guests.Add(guest);
        context.Events.Add(eventEntity);
        await context.SaveChangesAsync();

        var result = await repository.DeleteAsync(eventId, CancellationToken.None);

        result.ShouldBeTrue();
        context.Events.ShouldBeEmpty();
        context.Guests.ShouldBeEmpty();
        context.Information.ShouldBeEmpty();
    }

    [Fact]
    public async Task DeletesEventWithoutGuests_WhenFound()
    {
        using var context = CreateContext();
        var repository = new EventRepository(context);
        var eventId = "event-id-1";
        var eventEntity = new EventEntity
        {
            Id = eventId,
            Name = "Event 1"
        };
        context.Events.Add(eventEntity);
        await context.SaveChangesAsync();

        var result = await repository.DeleteAsync(eventId, CancellationToken.None);

        result.ShouldBeTrue();
        context.Events.ShouldBeEmpty();
    }

    [Fact]
    public async Task ReturnsFalse_WhenEventNotFound()
    {
        using var context = CreateContext();
        var repository = new EventRepository(context);
        var eventId = "non-existent-id";

        var result = await repository.DeleteAsync(eventId, CancellationToken.None);

        result.ShouldBeFalse();
    }

    private static TomeshelfMcmDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TomeshelfMcmDbContext>().UseSqlite($"DataSource={Guid.NewGuid():N};Mode=Memory;Cache=Shared")
                                                                          .Options;

        var context = new TomeshelfMcmDbContext(options);
        context.Database.OpenConnection();
        context.Database.EnsureCreated();

        return context;
    }
}