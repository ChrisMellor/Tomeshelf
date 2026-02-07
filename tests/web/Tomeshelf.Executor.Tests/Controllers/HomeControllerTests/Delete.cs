using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using Tomeshelf.Executor.Configuration;
using Tomeshelf.Executor.Controllers;
using Tomeshelf.Executor.Services;
using Tomeshelf.Executor.Tests.TestUtilities;
using Tomeshelf.ServiceDefaults;

namespace Tomeshelf.Executor.Tests.Controllers.HomeControllerTests;

public class Delete
{
    [Fact]
    public async Task Post_RemovesEndpoint()
    {
        // Arrange
        var options = new ExecutorOptions
        {
            Endpoints = new List<EndpointScheduleOptions>
            {
                new EndpointScheduleOptions
                {
                    Name = "DeleteMe",
                    Url = "https://example.test",
                    Cron = "0 0 * * * ?"
                }
            }
        };
        var controller = HomeControllerTestHarness.CreateController(options, new List<ApiServiceDescriptor>(), out var store, out var scheduler, out _, out _);
        A.CallTo(() => scheduler.RefreshAsync(A<ExecutorOptions>._, A<CancellationToken>._))
         .Returns(Task.CompletedTask);
        A.CallTo(() => store.SaveAsync(A<ExecutorOptions>._, A<CancellationToken>._))
         .Returns(Task.CompletedTask);

        // Act
        var result = await controller.Delete("DeleteMe", CancellationToken.None);

        // Assert
        result.ShouldBeOfType<RedirectToActionResult>();
        controller.TempData["StatusMessage"].ShouldBe("Deleted endpoint 'DeleteMe'.");
        options.Endpoints.ShouldBeEmpty();
    }
}
