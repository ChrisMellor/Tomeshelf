using FakeItEasy;
using Microsoft.Extensions.Logging;
using Quartz;
using Tomeshelf.Executor.Configuration;
using Tomeshelf.Executor.Jobs;
using Tomeshelf.Executor.Services;
using Tomeshelf.Executor.Tests.TestUtilities;

namespace Tomeshelf.Executor.Tests.Jobs.TriggerEndpointJobTests;

public class Execute
{
    [Fact]
    public async Task WhenEndpointDisabled_DoesNotPing()
    {
        var pingService = A.Fake<IEndpointPingService>();
        var options = new ExecutorOptions
        {
            Endpoints = new List<EndpointScheduleOptions>
            {
                new()
                {
                    Name = "Ping",
                    Url = "https://example.test",
                    Cron = "0 0 * * * ?",
                    Enabled = false
                }
            }
        };
        var job = new TriggerEndpointJob(pingService, new TestOptionsMonitor<ExecutorOptions>(options), A.Fake<ILogger<TriggerEndpointJob>>());
        var context = CreateContext("Ping");

        await job.Execute(context);

        A.CallTo(() => pingService.SendAsync(A<Uri>._, A<string>._, A<Dictionary<string, string>?>._, A<CancellationToken>._))
         .MustNotHaveHappened();
    }

    [Fact]
    public async Task WhenEndpointNameMissing_DoesNotPing()
    {
        var pingService = A.Fake<IEndpointPingService>();
        var options = new ExecutorOptions
        {
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
        var job = new TriggerEndpointJob(pingService, new TestOptionsMonitor<ExecutorOptions>(options), A.Fake<ILogger<TriggerEndpointJob>>());
        var context = CreateContext(string.Empty);

        await job.Execute(context);

        A.CallTo(() => pingService.SendAsync(A<Uri>._, A<string>._, A<Dictionary<string, string>?>._, A<CancellationToken>._))
         .MustNotHaveHappened();
    }

    [Fact]
    public async Task WhenEndpointValid_PingsConfiguredTarget()
    {
        var pingService = A.Fake<IEndpointPingService>();
        A.CallTo(() => pingService.SendAsync(A<Uri>._, A<string>._, A<Dictionary<string, string>?>._, A<CancellationToken>._))
         .Returns(new EndpointPingResult(true, 200, "OK", "body", TimeSpan.FromMilliseconds(10)));

        var headers = new Dictionary<string, string> { ["X-Test"] = "value" };
        var options = new ExecutorOptions
        {
            Endpoints = new List<EndpointScheduleOptions>
            {
                new()
                {
                    Name = "Ping",
                    Url = "https://example.test",
                    Cron = "0 0 * * * ?",
                    Method = "PUT",
                    Headers = headers
                }
            }
        };
        var job = new TriggerEndpointJob(pingService, new TestOptionsMonitor<ExecutorOptions>(options), A.Fake<ILogger<TriggerEndpointJob>>());
        var context = CreateContext("Ping");

        await job.Execute(context);

        A.CallTo(() => pingService.SendAsync(A<Uri>.That.Matches(uri => uri.ToString() == "https://example.test/"), "PUT", headers, A<CancellationToken>._))
         .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task WhenUrlInvalid_DoesNotPing()
    {
        var pingService = A.Fake<IEndpointPingService>();
        var options = new ExecutorOptions
        {
            Endpoints = new List<EndpointScheduleOptions>
            {
                new()
                {
                    Name = "Ping",
                    Url = "not-a-url",
                    Cron = "0 0 * * * ?"
                }
            }
        };
        var job = new TriggerEndpointJob(pingService, new TestOptionsMonitor<ExecutorOptions>(options), A.Fake<ILogger<TriggerEndpointJob>>());
        var context = CreateContext("Ping");

        await job.Execute(context);

        A.CallTo(() => pingService.SendAsync(A<Uri>._, A<string>._, A<Dictionary<string, string>?>._, A<CancellationToken>._))
         .MustNotHaveHappened();
    }

    private static IJobExecutionContext CreateContext(string? endpointName)
    {
        var context = A.Fake<IJobExecutionContext>();
        var map = new JobDataMap();
        if (endpointName is not null)
        {
            map[TriggerEndpointJob.EndpointNameKey] = endpointName;
        }

        A.CallTo(() => context.MergedJobDataMap)
         .Returns(map);
        A.CallTo(() => context.CancellationToken)
         .Returns(CancellationToken.None);

        return context;
    }
}