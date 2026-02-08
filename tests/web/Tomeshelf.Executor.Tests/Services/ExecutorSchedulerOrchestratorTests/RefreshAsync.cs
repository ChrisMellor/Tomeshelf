using FakeItEasy;
using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Impl.Matchers;
using Shouldly;
using Tomeshelf.Executor.Configuration;
using Tomeshelf.Executor.Jobs;
using Tomeshelf.Executor.Services;
using Tomeshelf.Executor.Tests.TestUtilities;

namespace Tomeshelf.Executor.Tests.Services.ExecutorSchedulerOrchestratorTests;

public class RefreshAsync
{
    [Fact]
    public async Task WhenDisabled_RemovesJobsAndStandby()
    {
        // Arrange
        var scheduler = A.Fake<IScheduler>();
        var staleJob = new JobKey("old-endpoint", "ExecutorEndpoints");
        A.CallTo(() => scheduler.GetJobKeys(A<GroupMatcher<JobKey>>._, A<CancellationToken>._))
         .Returns(Task.FromResult<IReadOnlyCollection<JobKey>>(new HashSet<JobKey> { staleJob }));
        A.CallTo(() => scheduler.DeleteJob(A<JobKey>._, A<CancellationToken>._))
         .Returns(Task.FromResult(true));
        A.CallTo(() => scheduler.Standby(A<CancellationToken>._))
         .Returns(Task.CompletedTask);

        var factory = A.Fake<ISchedulerFactory>();
        A.CallTo(() => factory.GetScheduler(A<CancellationToken>._))
         .Returns(Task.FromResult(scheduler));

        var orchestrator = new ExecutorSchedulerOrchestrator(factory, new TestOptionsMonitor<ExecutorOptions>(new ExecutorOptions()), A.Fake<ILogger<ExecutorSchedulerOrchestrator>>());
        var options = new ExecutorOptions
        {
            Enabled = false,
            Endpoints = new List<EndpointScheduleOptions>
            {
                new()
                {
                    Name = "Ping",
                    Url = "https://example.test",
                    Cron = "0 0 * * * ?"
                }
            }
        };

        // Act
        await orchestrator.RefreshAsync(options, CancellationToken.None);

        // Assert
        A.CallTo(() => scheduler.DeleteJob(staleJob, A<CancellationToken>._))
         .MustHaveHappenedOnceExactly();
        A.CallTo(() => scheduler.ScheduleJob(A<IJobDetail>._, A<IReadOnlyCollection<ITrigger>>._, true, A<CancellationToken>._))
         .MustNotHaveHappened();
        A.CallTo(() => scheduler.Standby(A<CancellationToken>._))
         .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task WhenEnabled_SchedulesEndpointsAndStarts()
    {
        // Arrange
        var scheduled = new List<(IJobDetail Job, ITrigger Trigger)>();
        var scheduler = A.Fake<IScheduler>();
        A.CallTo(() => scheduler.GetJobKeys(A<GroupMatcher<JobKey>>._, A<CancellationToken>._))
         .Returns(Task.FromResult<IReadOnlyCollection<JobKey>>(new HashSet<JobKey> { new("stale", "ExecutorEndpoints") }));
        A.CallTo(() => scheduler.DeleteJob(A<JobKey>._, A<CancellationToken>._))
         .Returns(Task.FromResult(true));
        A.CallTo(() => scheduler.ScheduleJob(A<IJobDetail>._, A<IReadOnlyCollection<ITrigger>>._, true, A<CancellationToken>._))
         .Invokes((IJobDetail job, IReadOnlyCollection<ITrigger> triggers, bool _, CancellationToken _) =>
          {
              scheduled.Add((job, triggers.Single()));
          })
         .Returns(Task.FromResult(DateTimeOffset.UtcNow));
        A.CallTo(() => scheduler.InStandbyMode)
         .Returns(true);
        A.CallTo(() => scheduler.IsStarted)
         .Returns(false);
        A.CallTo(() => scheduler.Start(A<CancellationToken>._))
         .Returns(Task.CompletedTask);

        var factory = A.Fake<ISchedulerFactory>();
        A.CallTo(() => factory.GetScheduler(A<CancellationToken>._))
         .Returns(Task.FromResult(scheduler));

        var options = new ExecutorOptions
        {
            Enabled = true,
            Endpoints = new List<EndpointScheduleOptions>
            {
                new()
                {
                    Name = "Alpha",
                    Url = "https://alpha.test",
                    Cron = "0 0 * * * ?",
                    Enabled = true
                },
                new()
                {
                    Name = "Beta",
                    Url = "https://beta.test",
                    Cron = "0 5 * * * ?",
                    Enabled = true
                },
                new()
                {
                    Name = "Invalid",
                    Url = "",
                    Cron = ""
                }
            }
        };

        var orchestrator = new ExecutorSchedulerOrchestrator(factory, new TestOptionsMonitor<ExecutorOptions>(options), A.Fake<ILogger<ExecutorSchedulerOrchestrator>>());

        // Act
        await orchestrator.RefreshAsync(options, CancellationToken.None);

        // Assert
        scheduled.Count.ShouldBe(2);
        scheduled.ShouldContain(item => (item.Job.Key.Name == "Alpha") && (item.Job.Key.Group == "ExecutorEndpoints"));
        scheduled.ShouldContain(item => (item.Job.Key.Name == "Beta") && (item.Job.Key.Group == "ExecutorEndpoints"));
        scheduled.All(item => item.Job.JobDataMap.GetString(TriggerEndpointJob.EndpointNameKey) == item.Job.Key.Name)
                 .ShouldBeTrue();

        var cronTrigger = scheduled.First()
                                   .Trigger
                                   .ShouldBeAssignableTo<ICronTrigger>();
        string.IsNullOrWhiteSpace(cronTrigger.CronExpressionString)
              .ShouldBeFalse();

        A.CallTo(() => scheduler.Start(A<CancellationToken>._))
         .MustHaveHappenedOnceExactly();
    }
}