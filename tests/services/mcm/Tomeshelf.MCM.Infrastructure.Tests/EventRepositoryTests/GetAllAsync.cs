using Microsoft.EntityFrameworkCore;
using Shouldly;
using Tomeshelf.MCM.Domain.Mcm;
using Tomeshelf.MCM.Infrastructure.Repositories;

namespace Tomeshelf.MCM.Infrastructure.Tests.EventRepositoryTests;

public class GetAllAsync
{
    /// <summary>
    ///     Returns the events ordered by name.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task ReturnsEventsOrderedByName()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new EventRepository(context);
        var event1 = new EventEntity
        {
            Id = "id-2",
            Name = "Event B"
        };
        var event2 = new EventEntity
        {
            Id = "id-1",
            Name = "Event A"
        };
        context.Events.AddRange(event1, event2);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetAllAsync(CancellationToken.None);

        // Assert
        result.Count.ShouldBe(2);
        result.First()
              .Name
              .ShouldBe("Event A");
        result.Last()
              .Name
              .ShouldBe("Event B");
    }

    /// <summary>
    ///     Creates the context.
    /// </summary>
    /// <returns>The result of the operation.</returns>
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