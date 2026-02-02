using System;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Tomeshelf.Executor.Configuration;
using Tomeshelf.Executor.Services;
using Tomeshelf.Executor.Tests.TestUtilities;

namespace Tomeshelf.Executor.Tests.Services;

public class ExecutorSchedulerHostedServiceTests
{
    [Fact]
    public async Task StartAsync_RefreshesSchedulerAndRegistersChangeHandler()
    {
        var orchestrator = A.Fake<IExecutorSchedulerOrchestrator>();
        A.CallTo(() => orchestrator.RefreshAsync(A<ExecutorOptions?>._, A<CancellationToken>._))
            .Returns(Task.CompletedTask);

        var options = new ExecutorOptions();
        var monitor = new TestOptionsMonitor<ExecutorOptions>(options);
        var service = new ExecutorSchedulerHostedService(orchestrator, monitor, A.Fake<ILogger<ExecutorSchedulerHostedService>>());

        await service.StartAsync(CancellationToken.None);

        A.CallTo(() => orchestrator.RefreshAsync(null, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task OptionsChange_TriggersRefresh()
    {
        var orchestrator = A.Fake<IExecutorSchedulerOrchestrator>();
        A.CallTo(() => orchestrator.RefreshAsync(A<ExecutorOptions?>._, A<CancellationToken>._))
            .Returns(Task.CompletedTask);

        var monitor = new TestOptionsMonitor<ExecutorOptions>(new ExecutorOptions());
        var service = new ExecutorSchedulerHostedService(orchestrator, monitor, A.Fake<ILogger<ExecutorSchedulerHostedService>>());
        await service.StartAsync(CancellationToken.None);

        var tcs = new TaskCompletionSource<bool>();
        A.CallTo(() => orchestrator.RefreshAsync(A<ExecutorOptions>._, A<CancellationToken>._))
            .Invokes(() => tcs.TrySetResult(true))
            .Returns(Task.CompletedTask);

        monitor.Set(new ExecutorOptions());

        await tcs.Task.WaitAsync(TimeSpan.FromSeconds(1));

        A.CallTo(() => orchestrator.RefreshAsync(A<ExecutorOptions>._, A<CancellationToken>._))
            .MustHaveHappened();
    }

    [Fact]
    public async Task StopAsync_DisposesSubscription()
    {
        var orchestrator = A.Fake<IExecutorSchedulerOrchestrator>();
        A.CallTo(() => orchestrator.RefreshAsync(A<ExecutorOptions?>._, A<CancellationToken>._))
            .Returns(Task.CompletedTask);

        var monitor = new TestOptionsMonitor<ExecutorOptions>(new ExecutorOptions());
        var service = new ExecutorSchedulerHostedService(orchestrator, monitor, A.Fake<ILogger<ExecutorSchedulerHostedService>>());
        await service.StartAsync(CancellationToken.None);

        await service.StopAsync(CancellationToken.None);
        monitor.Set(new ExecutorOptions());
        await Task.Delay(50);

        A.CallTo(() => orchestrator.RefreshAsync(A<ExecutorOptions?>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }
}
