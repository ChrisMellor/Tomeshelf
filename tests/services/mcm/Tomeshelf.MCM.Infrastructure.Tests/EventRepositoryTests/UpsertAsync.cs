using Microsoft.EntityFrameworkCore;
using Shouldly;
using Tomeshelf.MCM.Application.Models;
using Tomeshelf.MCM.Domain.Mcm;
using Tomeshelf.MCM.Infrastructure.Repositories;

namespace Tomeshelf.MCM.Infrastructure.Tests.EventRepositoryTests;

public class UpsertAsync
{
    [Fact]
    public async Task AddsEvent_WhenMissing()
    {
        using var context = CreateContext();
        var repository = new EventRepository(context);
        var model = new EventConfigModel
        {
            Id = "new-id",
            Name = "New Event"
        };

        var result = await repository.UpsertAsync(model, CancellationToken.None);

        result.ShouldBe(1);
        context.Events.ShouldHaveSingleItem();
        context.Events
               .First()
               .Name
               .ShouldBe("New Event");
    }

    [Fact]
    public async Task UpdatesEvent_WhenExists()
    {
        using var context = CreateContext();
        var repository = new EventRepository(context);
        var eventId = "existing-id";
        var existingEvent = new EventEntity
        {
            Id = eventId,
            Name = "Old Name"
        };
        context.Events.Add(existingEvent);
        await context.SaveChangesAsync();

        var model = new EventConfigModel
        {
            Id = eventId,
            Name = "Updated Name"
        };

        var result = await repository.UpsertAsync(model, CancellationToken.None);

        result.ShouldBe(1);
        context.Events.ShouldHaveSingleItem();
        context.Events
               .First()
               .Name
               .ShouldBe("Updated Name");
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