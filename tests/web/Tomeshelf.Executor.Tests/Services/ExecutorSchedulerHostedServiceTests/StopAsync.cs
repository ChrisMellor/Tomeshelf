using FakeItEasy;
using Microsoft.Extensions.Logging;
using Tomeshelf.Executor.Configuration;
using Tomeshelf.Executor.Services;
using Tomeshelf.Executor.Tests.TestUtilities;

namespace Tomeshelf.Executor.Tests.Services.ExecutorSchedulerHostedServiceTests;

public class StopAsync
{
    [Fact]
    public async Task DisposesSubscription()
    {
        // Arrange
        var orchestrator = A.Fake<IExecutorSchedulerOrchestrator>();
        A.CallTo(() => orchestrator.RefreshAsync(A<ExecutorOptions?>._, A<CancellationToken>._))
         .Returns(Task.CompletedTask);

        var monitor = new TestOptionsMonitor<ExecutorOptions>(new ExecutorOptions());
        var service = new ExecutorSchedulerHostedService(orchestrator, monitor, A.Fake<ILogger<ExecutorSchedulerHostedService>>());
        await service.StartAsync(CancellationToken.None);

        // Act
        await service.StopAsync(CancellationToken.None);
        monitor.Set(new ExecutorOptions());
        await Task.Delay(50);

        // Assert
        A.CallTo(() => orchestrator.RefreshAsync(A<ExecutorOptions?>._, A<CancellationToken>._))
         .MustHaveHappenedOnceExactly();
    }
}