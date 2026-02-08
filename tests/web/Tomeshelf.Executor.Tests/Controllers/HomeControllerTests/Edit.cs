using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using Shouldly;
using Tomeshelf.Executor.Configuration;
using Tomeshelf.Executor.Models;
using Tomeshelf.Executor.Services;
using Tomeshelf.Executor.Tests.TestUtilities;

namespace Tomeshelf.Executor.Tests.Controllers.HomeControllerTests;

public class Edit
{
    [Fact]
    public async Task Get_MissingEndpoint_Redirects()
    {
        // Arrange
        var controller = HomeControllerTestHarness.CreateController(new ExecutorOptions(), new List<ApiServiceDescriptor>(), out _, out _, out _, out _);

        // Act
        var result = await controller.Edit("missing", CancellationToken.None);

        // Assert
        result.ShouldBeOfType<RedirectToActionResult>();
    }

    [Fact]
    public async Task Post_Valid_UpdatesEndpoint()
    {
        // Arrange
        var options = new ExecutorOptions
        {
            Endpoints = new List<EndpointScheduleOptions>
            {
                new()
                {
                    Name = "Existing",
                    Url = "https://example.test",
                    Cron = "0 0 * * * ?",
                    Method = "POST"
                }
            }
        };
        var controller = HomeControllerTestHarness.CreateController(options, new List<ApiServiceDescriptor>(), out var store, out var scheduler, out _, out _);
        A.CallTo(() => scheduler.RefreshAsync(A<ExecutorOptions>._, A<CancellationToken>._))
         .Returns(Task.CompletedTask);
        A.CallTo(() => store.SaveAsync(A<ExecutorOptions>._, A<CancellationToken>._))
         .Returns(Task.CompletedTask);

        var model = new EndpointEditorModel
        {
            Name = "Updated",
            Url = "updated.test",
            Cron = "0 1 * * * ?",
            Method = "PUT",
            Enabled = true
        };

        // Act
        var result = await controller.Edit("Existing", "Existing", model, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<RedirectToActionResult>();
        controller.TempData["StatusMessage"]
                  .ShouldBe("Updated endpoint 'Updated'.");
        options.Endpoints[0]
               .Name
               .ShouldBe("Updated");
        options.Endpoints[0]
               .Url
               .ShouldBe("http://updated.test/");
    }
}