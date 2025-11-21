using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using Tomeshelf.Executor.Configuration;
using Tomeshelf.Executor.Services;

namespace Tomeshelf.Executor.Jobs;

public class TriggerEndpointJob(IEndpointPingService pingService, IOptionsMonitor<ExecutorOptions> executorOptions, ILogger<TriggerEndpointJob> logger) : IJob
{
    public const string EndpointNameKey = "Executor.EndpointName";
    public const string HttpClientName = "Executor.EndpointClient";
    private readonly IOptionsMonitor<ExecutorOptions> _executorOptions = executorOptions;

    private readonly IEndpointPingService _pingService = pingService;
    private readonly ILogger<TriggerEndpointJob> _logger = logger;

    public async Task Execute(IJobExecutionContext context)
    {
        var jobDataMap = context.MergedJobDataMap;
        var endpointName = jobDataMap.GetString(EndpointNameKey);
        if (string.IsNullOrWhiteSpace(endpointName))
        {
            _logger.LogWarning("Endpoint name missing from job data.");

            return;
        }

        var endpoint = _executorOptions.CurrentValue.Endpoints.FirstOrDefault(ep => string.Equals(ep.Name, endpointName, StringComparison.OrdinalIgnoreCase));

        if (endpoint is null)
        {
            _logger.LogWarning("No configuration found for endpoint '{EndpointName}'.", endpointName);

            return;
        }

        if (!endpoint.Enabled)
        {
            _logger.LogInformation("Endpoint '{EndpointName}' is disabled; skipping execution.", endpointName);

            return;
        }

        if (!Uri.TryCreate(endpoint.Url, UriKind.Absolute, out var uri))
        {
            _logger.LogWarning("Invalid URL '{Url}' configured for endpoint '{EndpointName}'.", endpoint.Url, endpointName);

            return;
        }

        try
        {
            var result = await _pingService.SendAsync(uri, endpoint.Method, endpoint.Headers, context.CancellationToken);
            if (result.Success && result.StatusCode.HasValue)
            {
                _logger.LogInformation("Executed endpoint '{EndpointName}' with status {StatusCode}.", endpointName, result.StatusCode.Value);
            }
            else if (result.StatusCode.HasValue)
            {
                var responseBody = result.Body ?? result.Message;
                _logger.LogError("Endpoint '{EndpointName}' responded with status {StatusCode}. Body: {Body}", endpointName, result.StatusCode.Value, responseBody);
            }
            else
            {
                _logger.LogError("Endpoint '{EndpointName}' failed: {Message}", endpointName, result.Message);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Failed to execute endpoint '{EndpointName}'.", endpointName);
        }
    }

    public static TimeZoneInfo ResolveTimeZone(string? timeZoneId)
    {
        if (string.IsNullOrWhiteSpace(timeZoneId))
        {
            return TimeZoneInfo.Utc;
        }

        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.Utc;
        }
        catch (InvalidTimeZoneException)
        {
            return TimeZoneInfo.Utc;
        }
    }
}
