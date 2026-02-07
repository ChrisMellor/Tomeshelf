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

public class Toggle
{
    [Fact]
    public async Task Post_UpdatesOptionsAndRefreshes()
    {
        // Arrange
        var options = new ExecutorOptions { Enabled = false };
        var controller = HomeControllerTestHarness.CreateController(options, new List<ApiServiceDescriptor>(), out var store, out var scheduler, out _, out _);
        A.CallTo(() => store.SaveAsync(A<ExecutorOptions>._, A<CancellationToken>._))
         .Returns(Task.CompletedTask);
        A.CallTo(() => scheduler.RefreshAsync(A<ExecutorOptions>._, A<CancellationToken>._))
         .Returns(Task.CompletedTask);

        // Act
        var result = await controller.Toggle(true, CancellationToken.None);

        // Assert
        options.Enabled.ShouldBeTrue();
        controller.TempData["StatusMessage"].ShouldBe("Scheduler enabled.");
        result.ShouldBeOfType<RedirectToActionResult>();
    }
}
