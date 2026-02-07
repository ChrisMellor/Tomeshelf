using FakeItEasy;
using Shouldly;
using Tomeshelf.MCM.Application.Models;
using Tomeshelf.MCM.Application.Records;
using Tomeshelf.MCM.Application.Tests.TestUtilities;

namespace Tomeshelf.MCM.Application.Tests.GuestsServiceTests;

public class GetAsync
{
    [Fact]
    public async Task ReturnsPagedResult()
    {
        var (service, _, _, repository) = GuestsServiceTestHarness.CreateService();
        var model = new EventConfigModel { Id = "test-event" };
        var snapshot = new GuestSnapshot(1, new List<GuestListItem> { new(Guid.NewGuid(), "Test Guest", "Test Description", "http://example.com/profile", "http://example.com/image.jpg", DateTimeOffset.UtcNow, null, false) });

        A.CallTo(() => repository.GetPageAsync(model.Id, 0, 10, false, CancellationToken.None))
         .Returns(Task.FromResult(snapshot));

        var result = await service.GetAsync(model, 0, 10, false, CancellationToken.None);

        result.ShouldNotBeNull();
        result.Total.ShouldBe(1);
        result.Items.ShouldHaveSingleItem();
        result.Items[0]
              .Name
              .ShouldBe("Test Guest");
    }
}