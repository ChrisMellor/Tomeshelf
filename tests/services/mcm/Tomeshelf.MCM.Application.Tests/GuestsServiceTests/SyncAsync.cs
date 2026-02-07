using FakeItEasy;
using Tomeshelf.MCM.Application.Models;
using Tomeshelf.MCM.Application.Records;
using Tomeshelf.MCM.Application.Tests.TestUtilities;
using Tomeshelf.MCM.Domain.Mcm;

namespace Tomeshelf.MCM.Application.Tests.GuestsServiceTests;

public class SyncAsync
{
    [Fact]
    public async Task ReturnsNull_WhenEventDoesNotExist()
    {
        // Arrange
        var (service, _, _, repository) = GuestsServiceTestHarness.CreateService();
        var model = new EventConfigModel { Id = "test-event" };
        A.CallTo(() => repository.GetEventWithGuestsAsync(model.Id, CancellationToken.None))
         .Returns(Task.FromResult<EventEntity?>(null));

        // Act
        var result = await service.SyncAsync(model, CancellationToken.None);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task AddsNewGuest()
    {
        // Arrange
        var (service, client, mapper, repository) = GuestsServiceTestHarness.CreateService();
        var model = new EventConfigModel { Id = "test-event" };
        var eventEntity = new EventEntity { Id = model.Id, Name = "Test Event", Guests = new List<GuestEntity>() };
        var guestRecord = new GuestRecord("Test Guest", "Test Description", "http://example.com/profile", "http://example.com/image.jpg");
        var guestEntity = new GuestEntity { Information = new GuestInfoEntity { FirstName = "Test", LastName = "Guest" } };

        A.CallTo(() => repository.GetEventWithGuestsAsync(model.Id, CancellationToken.None))
         .Returns(Task.FromResult<EventEntity?>(eventEntity));
        A.CallTo(() => client.FetchGuestsAsync(model.Id, CancellationToken.None))
         .Returns(Task.FromResult<IReadOnlyList<GuestRecord>>(new[] { guestRecord }));
        A.CallTo(() => mapper.GetGuestKey(A<GuestEntity>._)).Returns("TestGuest");
        A.CallTo(() => mapper.CloneForEvent(model.Id, A<GuestEntity>._)).Returns(guestEntity);

        // Act
        var result = await service.SyncAsync(model, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result!.Added.ShouldBe(1);
        A.CallTo(() => repository.AddGuest(guestEntity)).MustHaveHappenedOnceExactly();
        A.CallTo(() => repository.SaveChangesAsync(CancellationToken.None)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task UpdatesGuest()
    {
        // Arrange
        var (service, client, mapper, repository) = GuestsServiceTestHarness.CreateService();
        var model = new EventConfigModel { Id = "test-event" };
        var guestEntity = new GuestEntity { Information = new GuestInfoEntity { FirstName = "Test", LastName = "Guest" } };
        var eventEntity = new EventEntity { Id = model.Id, Name = "Test Event", Guests = new List<GuestEntity> { guestEntity } };
        var guestRecord = new GuestRecord("Test Guest", "Test Description", "http://example.com/profile", "http://example.com/image.jpg");

        A.CallTo(() => repository.GetEventWithGuestsAsync(model.Id, CancellationToken.None))
         .Returns(Task.FromResult<EventEntity?>(eventEntity));
        A.CallTo(() => client.FetchGuestsAsync(model.Id, CancellationToken.None))
         .Returns(Task.FromResult<IReadOnlyList<GuestRecord>>(new[] { guestRecord }));
        A.CallTo(() => mapper.GetGuestKey(A<GuestEntity>._)).Returns("TestGuest");
        A.CallTo(() => mapper.UpdateGuest(guestEntity, A<GuestEntity>._)).Returns(true);

        // Act
        var result = await service.SyncAsync(model, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result!.Updated.ShouldBe(1);
        A.CallTo(() => repository.SaveChangesAsync(CancellationToken.None)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task RemovesGuest()
    {
        // Arrange
        var (service, client, mapper, repository) = GuestsServiceTestHarness.CreateService();
        var model = new EventConfigModel { Id = "test-event" };
        var guestEntity = new GuestEntity { Information = new GuestInfoEntity { FirstName = "Test", LastName = "Guest" } };
        var eventEntity = new EventEntity { Id = model.Id, Name = "Test Event", Guests = new List<GuestEntity> { guestEntity } };

        A.CallTo(() => repository.GetEventWithGuestsAsync(model.Id, CancellationToken.None))
         .Returns(Task.FromResult<EventEntity?>(eventEntity));
        A.CallTo(() => client.FetchGuestsAsync(model.Id, CancellationToken.None))
         .Returns(Task.FromResult<IReadOnlyList<GuestRecord>>(Array.Empty<GuestRecord>()));
        A.CallTo(() => mapper.GetGuestKey(A<GuestEntity>._)).Returns("TestGuest");

        var before = DateTimeOffset.UtcNow;

        // Act
        var result = await service.SyncAsync(model, CancellationToken.None);

        // Assert
        var after = DateTimeOffset.UtcNow;
        result.ShouldNotBeNull();
        result!.Removed.ShouldBe(1);
        guestEntity.IsDeleted.ShouldBeTrue();
        guestEntity.RemovedAt.ShouldNotBeNull();
        guestEntity.RemovedAt!.Value.ShouldBeInRange(before, after);
        A.CallTo(() => repository.SaveChangesAsync(CancellationToken.None)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task SavesChanges_WhenNoUpdates()
    {
        // Arrange
        var (service, client, mapper, repository) = GuestsServiceTestHarness.CreateService();
        var model = new EventConfigModel { Id = "test-event" };
        var guestEntity = new GuestEntity { Information = new GuestInfoEntity { FirstName = "Test", LastName = "Guest" } };
        var eventEntity = new EventEntity { Id = model.Id, Name = "Test Event", Guests = new List<GuestEntity> { guestEntity } };
        var guestRecord = new GuestRecord("Test Guest", "Test Description", "http://example.com/profile", "http://example.com/image.jpg");

        A.CallTo(() => repository.GetEventWithGuestsAsync(model.Id, CancellationToken.None))
         .Returns(Task.FromResult<EventEntity?>(eventEntity));
        A.CallTo(() => client.FetchGuestsAsync(model.Id, CancellationToken.None))
         .Returns(Task.FromResult<IReadOnlyList<GuestRecord>>(new[] { guestRecord }));
        A.CallTo(() => mapper.GetGuestKey(A<GuestEntity>._)).Returns("TestGuest");
        A.CallTo(() => mapper.UpdateGuest(guestEntity, A<GuestEntity>._)).Returns(false);

        // Act
        var result = await service.SyncAsync(model, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result!.Added.ShouldBe(0);
        result.Updated.ShouldBe(0);
        result.Removed.ShouldBe(0);
        A.CallTo(() => repository.SaveChangesAsync(CancellationToken.None)).MustHaveHappenedOnceExactly();
    }
}
