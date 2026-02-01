using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.MCM.Application.Features.Events.Queries;
using Tomeshelf.MCM.Application.Models;
using Tomeshelf.MCM.Application.Services;
using Xunit;

namespace Tomeshelf.MCM.Application.Tests.Features.Events.Queries;

public class GetEventsQueryHandlerTests
{
    private readonly Mock<IEventService> _mockEventService;
    private readonly GetEventsQueryHandler _handler;

    public GetEventsQueryHandlerTests()
    {
        _mockEventService = new Mock<IEventService>();
        _handler = new GetEventsQueryHandler(_mockEventService.Object);
    }

    [Fact]
    public async Task Handle_ValidQuery_CallsGetAllAsyncAndReturnsResult()
    {
        // Arrange
        var expectedEvents = new List<EventConfigModel>
        {
            new() { Id = "event1", Name = "Event 1" },
            new() { Id = "event2", Name = "Event 2" }
        };

        _mockEventService
            .Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedEvents);

        var query = new GetEventsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(expectedEvents, result);
        _mockEventService.Verify(s => s.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
