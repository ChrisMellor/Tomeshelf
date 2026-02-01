using Moq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.MCM.Application.Contracts;
using Tomeshelf.MCM.Application.Features.Guests.Queries;
using Tomeshelf.MCM.Application.Models;
using Tomeshelf.MCM.Application.Services;
using Xunit;

namespace Tomeshelf.MCM.Application.Tests.Features.Guests.Queries;

public class GetGuestsQueryHandlerTests
{
    [Fact]
    public async Task Handle_BuildsEventConfigModelAndCallsService()
    {
        // Arrange
        var service = new Mock<IGuestsService>();
        var handler = new GetGuestsQueryHandler(service.Object);
        var query = new GetGuestsQuery("event-1", 2, 25, "Event One", true);
        var expected = new PagedResult<GuestDto>(0, new List<GuestDto>(), 2, 25);

        service.Setup(s => s.GetAsync(It.IsAny<EventConfigModel>(), 2, 25, true, It.IsAny<CancellationToken>()))
               .ReturnsAsync(expected);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Same(expected, result);
        service.Verify(s => s.GetAsync(It.Is<EventConfigModel>(model => model.Id == "event-1" && model.Name == "Event One"), 2, 25, true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_UsesEmptyNameWhenMissing()
    {
        // Arrange
        var service = new Mock<IGuestsService>();
        var handler = new GetGuestsQueryHandler(service.Object);
        var query = new GetGuestsQuery("event-2", 0, 10, null, false);
        var expected = new PagedResult<GuestDto>(0, new List<GuestDto>(), 0, 10);

        service.Setup(s => s.GetAsync(It.IsAny<EventConfigModel>(), 0, 10, false, It.IsAny<CancellationToken>()))
               .ReturnsAsync(expected);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Same(expected, result);
        service.Verify(s => s.GetAsync(It.Is<EventConfigModel>(model => model.Id == "event-2" && model.Name == string.Empty), 0, 10, false, It.IsAny<CancellationToken>()), Times.Once);
    }
}
