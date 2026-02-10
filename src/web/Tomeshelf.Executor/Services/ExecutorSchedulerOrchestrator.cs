using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using Quartz.Impl.Matchers;
using Tomeshelf.Executor.Configuration;
using Tomeshelf.Executor.Jobs;

namespace Tomeshelf.Executor.Services;

public sealed class ExecutorSchedulerOrchestrator : IExecutorSchedulerOrchestrator
{
    private const string JobGroup = "ExecutorEndpoints";
    private readonly IOptionsMonitor<ExecutorOptions> _executorOptions;
    private readonly ILogger<ExecutorSchedulerOrchestrator> _logger;

    private readonly ISchedulerFactory _schedulerFactory;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ExecutorSchedulerOrchestrator" /> class.
    /// </summary>
    /// <param name="schedulerFactory">The scheduler factory.</param>
    /// <param name="executorOptions">The executor options.</param>
    /// <param name="logger">The logger.</param>
    public ExecutorSchedulerOrchestrator(ISchedulerFactory schedulerFactory, IOptionsMonitor<ExecutorOptions> executorOptions, ILogger<ExecutorSchedulerOrchestrator> logger)
    {
        _schedulerFactory = schedulerFactory;
        _executorOptions = executorOptions;
        _logger = logger;
    }

    /// <summary>
    ///     Refreshs asynchronously.
    /// </summary>
    /// <param name="options">The options.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task RefreshAsync(ExecutorOptions? options = null, CancellationToken cancellationToken = default)
    {
        var scheduler = await _schedulerFactory.GetScheduler(cancellationToken);
        options ??= _executorOptions.CurrentValue;

        var desiredEndpoints = options.Enabled
            ? options.Endpoints
                     .Where(IsValid)
                     .ToList()
            : Array.Empty<EndpointScheduleOptions>()
                   .ToList();

        var existingJobKeys = await scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(JobGroup), cancellationToken);
        var desiredJobNames = new HashSet<string>(desiredEndpoints.Select(ep => ep.Name), StringComparer.OrdinalIgnoreCase);

        foreach (var jobKey in existingJobKeys)
        {
            if (!desiredJobNames.Contains(jobKey.Name))
            {
                await scheduler.DeleteJob(jobKey, cancellationToken);
            }
        }

        foreach (var endpoint in desiredEndpoints)
        {
            var job = JobBuilder.Create<TriggerEndpointJob>()
                                .WithIdentity(endpoint.Name, JobGroup)
                                .WithDescription($"Executes {endpoint.Url}")
                                .UsingJobData(TriggerEndpointJob.EndpointNameKey, endpoint.Name)
                                .Build();

            var trigger = TriggerBuilder.Create()
                                        .WithIdentity($"{endpoint.Name}.trigger", JobGroup)
                                        .ForJob(job)
                                        .WithDescription($"Cron schedule: {endpoint.Cron}")
                                        .WithCronSchedule(endpoint.Cron, cron =>
                                         {
                                             cron.InTimeZone(TriggerEndpointJob.ResolveTimeZone(endpoint.TimeZone));
                                         })
                                        .Build();

            await scheduler.ScheduleJob(job, new HashSet<ITrigger> { trigger }, true, cancellationToken);
        }

        if (!options.Enabled)
        {
            _logger.LogInformation("Executor scheduler is disabled. Putting scheduler into standby.");
            await scheduler.Standby(cancellationToken);

            return;
        }

        _logger.LogInformation("Executor scheduler synchronized with {Count} endpoints.", desiredEndpoints.Count);
        if (scheduler.InStandbyMode || !scheduler.IsStarted)
        {
            await scheduler.Start(cancellationToken);
        }
    }

    /// <summary>
    ///     Determines whether the specified endpoint is valid.
    /// </summary>
    /// <param name="endpoint">The endpoint.</param>
    /// <returns>True if the condition is met; otherwise, false.</returns>
    private static bool IsValid(EndpointScheduleOptions endpoint)
    {
        return endpoint.Enabled && !string.IsNullOrWhiteSpace(endpoint.Name) && !string.IsNullOrWhiteSpace(endpoint.Url) && !string.IsNullOrWhiteSpace(endpoint.Cron);
    }
}