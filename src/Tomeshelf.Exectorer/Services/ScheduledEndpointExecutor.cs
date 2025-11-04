using Microsoft.Extensions.Options;
using Tomeshelf.Executor.Models;
using Tomeshelf.Executor.Options;

namespace Tomeshelf.Executor.Services;

/// <summary>
///     Background service responsible for executing scheduled endpoints.
/// </summary>
public sealed class ScheduledEndpointExecutor : BackgroundService
{
    private static readonly TimeSpan MinimumSleepWindow = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan ExecutionDriftTolerance = TimeSpan.FromSeconds(1);

    private readonly EndpointCatalog _catalog;
    private readonly ILogger<ScheduledEndpointExecutor> _logger;
    private readonly ExecutorOptions _options;
    private readonly IServiceScopeFactory _scopeFactory;

    public ScheduledEndpointExecutor(EndpointCatalog catalog, IServiceScopeFactory scopeFactory, IOptions<ExecutorOptions> options, ILogger<ScheduledEndpointExecutor> logger)
    {
        _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.EnableScheduling)
        {
            _logger.LogInformation("Executor scheduling is disabled via configuration.");

            return;
        }

        var scheduledEndpoints = BuildScheduleStates();
        if (scheduledEndpoints.Count == 0)
        {
            _logger.LogInformation("No Executor endpoints have schedules configured.");

            return;
        }

        _logger.LogInformation("Initialised Executor scheduler with {Count} endpoint(s).", scheduledEndpoints.Count);

        while (!stoppingToken.IsCancellationRequested)
        {
            var nextState = GetNextState(scheduledEndpoints);
            if (nextState is null || nextState.NextUtc is null)
            {
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken)
                          .ConfigureAwait(false);

                continue;
            }

            var now = DateTimeOffset.UtcNow;
            var delay = nextState.NextUtc.Value - now;
            if (delay > MinimumSleepWindow)
            {
                try
                {
                    await Task.Delay(delay, stoppingToken)
                              .ConfigureAwait(false);
                }
                catch (TaskCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
            }

            now = DateTimeOffset.UtcNow;
            var dueStates = scheduledEndpoints.Where(s => s.NextUtc is not null && (s.NextUtc.Value <= (now + ExecutionDriftTolerance)))
                                              .ToList();

            foreach (var state in dueStates)
            {
                await ExecuteStateAsync(state, stoppingToken)
                       .ConfigureAwait(false);
                state.NextUtc = CalculateNextOccurrence(state, now);
            }
        }
    }

    private List<ScheduleState> BuildScheduleStates()
    {
        var descriptors = _catalog.GetDescriptors()
                                  .Where(d => d.CronExpression is not null)
                                  .ToList();

        var list = new List<ScheduleState>(descriptors.Count);
        var now = DateTimeOffset.UtcNow;

        foreach (var descriptor in descriptors)
        {
            var timeZone = descriptor.TimeZone ?? TimeZoneInfo.Utc;
            var next = descriptor.CronExpression!.GetNextValidTimeAfter(now);
            if (next is null)
            {
                continue;
            }

            list.Add(new ScheduleState(descriptor, timeZone) { NextUtc = next });
        }

        return list;
    }

    private static ScheduleState? GetNextState(IReadOnlyCollection<ScheduleState> states)
    {
        return states.Where(s => s.NextUtc is not null)
                     .OrderBy(s => s.NextUtc)
                     .FirstOrDefault();
    }

    private async Task ExecuteStateAsync(ScheduleState state, CancellationToken stoppingToken)
    {
        var cronExpression = state.Descriptor.CronExpression?.CronExpressionString ?? "<none>";
        _logger.LogInformation("Executing scheduled endpoint {EndpointId} (cron: {Cron}) at {UtcNow}.", state.Descriptor.Id, cronExpression, DateTimeOffset.UtcNow);

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var executor = scope.ServiceProvider.GetRequiredService<EndpointExecutor>();

            var result = await executor.ExecuteAsync(state.Descriptor, null, stoppingToken)
                                       .ConfigureAwait(false);

            if (!result.Success)
            {
                _logger.LogWarning("Scheduled endpoint {EndpointId} completed with status {StatusCode} ({Reason}).", result.EndpointId, result.StatusCode, result.ReasonPhrase);
            }
            else
            {
                _logger.LogInformation("Scheduled endpoint {EndpointId} succeeded with status {StatusCode}.", result.EndpointId, result.StatusCode);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Scheduled execution for endpoint {EndpointId} cancelled.", state.Descriptor.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Scheduled execution for endpoint {EndpointId} failed.", state.Descriptor.Id);
        }
    }

    private static DateTimeOffset? CalculateNextOccurrence(ScheduleState state, DateTimeOffset reference)
    {
        var effectiveReference = reference + ExecutionDriftTolerance;

        return state.Descriptor.CronExpression!.GetNextValidTimeAfter(effectiveReference);
    }

    private sealed class ScheduleState
    {
        public ScheduleState(EndpointDescriptor descriptor, TimeZoneInfo timeZone)
        {
            Descriptor = descriptor;
            TimeZone = timeZone;
        }

        public EndpointDescriptor Descriptor { get; }

        public TimeZoneInfo TimeZone { get; }

        public DateTimeOffset? NextUtc { get; set; }
    }
}