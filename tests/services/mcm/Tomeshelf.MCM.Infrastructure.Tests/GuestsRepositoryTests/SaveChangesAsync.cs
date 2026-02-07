using Microsoft.EntityFrameworkCore;
using Tomeshelf.MCM.Domain.Mcm;
using Tomeshelf.MCM.Infrastructure.Repositories;

namespace Tomeshelf.MCM.Infrastructure.Tests.GuestsRepositoryTests;

public class SaveChangesAsync
{
    [Fact]
    public async Task PersistsChanges()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new GuestsRepository(context);
        var guest = new GuestEntity { Id = Guid.NewGuid(), EventId = "test-event" };
        repository.AddGuest(guest);

        // Act
        await repository.SaveChangesAsync(CancellationToken.None);

        // Assert
        var savedGuest = await context.Guests.FindAsync(guest.Id);
        savedGuest.ShouldNotBeNull();
        savedGuest.ShouldBe(guest);
    }

    private static TomeshelfMcmDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TomeshelfMcmDbContext>()
                      .UseInMemoryDatabase(Guid.NewGuid().ToString())
                      .Options;

        return new TomeshelfMcmDbContext(options);
    }
}
