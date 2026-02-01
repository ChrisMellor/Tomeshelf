using Moq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.MCM.Application.Abstractions.Persistence;
using Tomeshelf.MCM.Application.Models;
using Tomeshelf.MCM.Application.Services;
using Tomeshelf.MCM.Domain.Mcm;
using Xunit;

namespace Tomeshelf.MCM.Application.Tests.Services;

public class EventServiceTests
{
    [Fact]
    public async Task GetAllAsync_MapsEntitiesToModels()
    {
        // Arrange
        var repository = new Mock<IEventRepository>();
        var service = new EventService(repository.Object);
        var entities = new List<EventEntity>
        {
            new() { Id = "event-1", Name = "Event One" },
            new() { Id = "event-2", Name = "Event Two" }
        };

        repository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                  .ReturnsAsync(entities);

        // Act
        var result = await service.GetAllAsync(CancellationToken.None);

        // Assert
        Assert.Collection(result,
            item =>
            {
                Assert.Equal("event-1", item.Id);
                Assert.Equal("Event One", item.Name);
            },
            item =>
            {
                Assert.Equal("event-2", item.Id);
                Assert.Equal("Event Two", item.Name);
            });
    }

    [Fact]
    public async Task UpsertAsync_CallsRepository()
    {
        // Arrange
        var repository = new Mock<IEventRepository>();
        var service = new EventService(repository.Object);
        var model = new EventConfigModel { Id = "event-1", Name = "Event One" };

        // Act
        await service.UpsertAsync(model, CancellationToken.None);

        // Assert
        repository.Verify(r => r.UpsertAsync(model, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_CallsRepositoryAndReturnsResult()
    {
        // Arrange
        var repository = new Mock<IEventRepository>();
        var service = new EventService(repository.Object);
        repository.Setup(r => r.DeleteAsync("event-1", It.IsAny<CancellationToken>()))
                  .ReturnsAsync(true);

        // Act
        var result = await service.DeleteAsync("event-1", CancellationToken.None);

        // Assert
        Assert.True(result);
        repository.Verify(r => r.DeleteAsync("event-1", It.IsAny<CancellationToken>()), Times.Once);
    }
}
