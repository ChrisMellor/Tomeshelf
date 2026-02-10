using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using Shouldly;
using Tomeshelf.Executor.Configuration;
using Tomeshelf.Executor.Services;
using Tomeshelf.Executor.Tests.TestUtilities;

namespace Tomeshelf.Executor.Tests.Controllers.HomeControllerTests;

public class Toggle
{
    /// <summary>
    ///     Updates options and refreshes when using POST.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
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
        controller.TempData["StatusMessage"]
                  .ShouldBe("Scheduler enabled.");
        result.ShouldBeOfType<RedirectToActionResult>();
    }
}