using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Impl.Matchers;
using Tomeshelf.Executor.Configuration;
using Tomeshelf.Executor.Jobs;
using Tomeshelf.Executor.Services;
using Tomeshelf.Executor.Tests.TestUtilities;

namespace Tomeshelf.Executor.Tests.Services;

public class ExecutorSchedulerOrchestratorTests
{
    [Fact]
    public async Task RefreshAsync_WhenDisabled_RemovesJobsAndStandby()
    {
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
                new EndpointScheduleOptions
                {
                    Name = "Ping",
                    Url = "https://example.test",
                    Cron = "0 0 * * * ?"
                }
            }
        };

        await orchestrator.RefreshAsync(options, CancellationToken.None);

        A.CallTo(() => scheduler.DeleteJob(staleJob, A<CancellationToken>._))
         .MustHaveHappenedOnceExactly();
        A.CallTo(() => scheduler.ScheduleJob(A<IJobDetail>._, A<IReadOnlyCollection<ITrigger>>._, true, A<CancellationToken>._))
         .MustNotHaveHappened();
        A.CallTo(() => scheduler.Standby(A<CancellationToken>._))
         .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task RefreshAsync_WhenEnabled_SchedulesEndpointsAndStarts()
    {
        var scheduled = new List<(IJobDetail Job, ITrigger Trigger)>();
        var scheduler = A.Fake<IScheduler>();
        A.CallTo(() => scheduler.GetJobKeys(A<GroupMatcher<JobKey>>._, A<CancellationToken>._))
         .Returns(Task.FromResult<IReadOnlyCollection<JobKey>>(new HashSet<JobKey> { new JobKey("stale", "ExecutorEndpoints") }));
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
                new EndpointScheduleOptions
                {
                    Name = "Alpha",
                    Url = "https://alpha.test",
                    Cron = "0 0 * * * ?",
                    Enabled = true
                },
                new EndpointScheduleOptions
                {
                    Name = "Beta",
                    Url = "https://beta.test",
                    Cron = "0 5 * * * ?",
                    Enabled = true
                },
                new EndpointScheduleOptions
                {
                    Name = "Invalid",
                    Url = "",
                    Cron = ""
                }
            }
        };

        var orchestrator = new ExecutorSchedulerOrchestrator(factory, new TestOptionsMonitor<ExecutorOptions>(options), A.Fake<ILogger<ExecutorSchedulerOrchestrator>>());

        await orchestrator.RefreshAsync(options, CancellationToken.None);

        scheduled.Should()
                 .HaveCount(2);
        scheduled.Should()
                 .Contain(item => (item.Job.Key.Name == "Alpha") && (item.Job.Key.Group == "ExecutorEndpoints"));
        scheduled.Should()
                 .Contain(item => (item.Job.Key.Name == "Beta") && (item.Job.Key.Group == "ExecutorEndpoints"));
        scheduled.All(item => item.Job.JobDataMap.GetString(TriggerEndpointJob.EndpointNameKey) == item.Job.Key.Name)
                 .Should()
                 .BeTrue();

        var cronTrigger = scheduled.First()
                                   .Trigger
                                   .Should()
                                   .BeAssignableTo<ICronTrigger>()
                                   .Subject;
        cronTrigger.CronExpressionString
                   .Should()
                   .NotBeNullOrWhiteSpace();

        A.CallTo(() => scheduler.Start(A<CancellationToken>._))
         .MustHaveHappenedOnceExactly();
    }
}