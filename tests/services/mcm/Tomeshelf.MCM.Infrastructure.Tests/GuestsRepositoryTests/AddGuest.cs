using Microsoft.EntityFrameworkCore;
using Shouldly;
using Tomeshelf.MCM.Domain.Mcm;
using Tomeshelf.MCM.Infrastructure.Repositories;

namespace Tomeshelf.MCM.Infrastructure.Tests.GuestsRepositoryTests;

public class AddGuest
{
    [Fact]
    public void AddsGuestToContext()
    {
        using var context = CreateContext();
        var repository = new GuestsRepository(context);
        var guest = new GuestEntity
        {
            Id = Guid.NewGuid(),
            EventId = "test-event"
        };

        repository.AddGuest(guest);

        var entry = context.ChangeTracker
                           .Entries<GuestEntity>()
                           .Single();
        entry.Entity.ShouldBe(guest);
    }

    private static TomeshelfMcmDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TomeshelfMcmDbContext>().UseInMemoryDatabase(Guid.NewGuid()
                                                                                                   .ToString())
                                                                          .Options;

        return new TomeshelfMcmDbContext(options);
    }
}