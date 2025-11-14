using Microsoft.Extensions.Options;
using Quartz;
using Tomeshelf.Executor.Configuration;

namespace Tomeshelf.Executor.Jobs;

public class TriggerEndpointJob(IHttpClientFactory httpClientFactory, IOptionsMonitor<ExecutorOptions> executorOptions, ILogger<TriggerEndpointJob> logger) : IJob
{
    public const string EndpointNameKey = "Executor.EndpointName";
    public const string HttpClientName = "Executor.EndpointClient";
    private readonly IOptionsMonitor<ExecutorOptions> _executorOptions = executorOptions;

    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
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

        var httpMethod = CreateMethod(endpoint.Method);
        var client = _httpClientFactory.CreateClient(HttpClientName);

        using var request = new HttpRequestMessage(httpMethod, uri);
        AddHeaders(endpoint, request);

        try
        {
            var response = await client.SendAsync(request, context.CancellationToken);
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Executed endpoint '{EndpointName}' with status {StatusCode}.", endpointName, (int)response.StatusCode);
            }
            else
            {
                var body = await response.Content.ReadAsStringAsync(context.CancellationToken);
                _logger.LogError("Endpoint '{EndpointName}' responded with status {StatusCode}. Body: {Body}", endpointName, (int)response.StatusCode, body);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Failed to execute endpoint '{EndpointName}'.", endpointName);
        }
    }

    private static HttpMethod CreateMethod(string? methodName)
    {
        if (string.IsNullOrWhiteSpace(methodName))
        {
            return HttpMethod.Post;
        }

        try
        {
            return new HttpMethod(methodName);
        }
        catch (FormatException)
        {
            return HttpMethod.Post;
        }
    }

    private static void AddHeaders(EndpointScheduleOptions endpoint, HttpRequestMessage request)
    {
        if (endpoint.Headers is null)
        {
            return;
        }

        foreach (var header in endpoint.Headers)
        {
            if (request.Headers.TryAddWithoutValidation(header.Key, header.Value))
            {
                continue;
            }

            request.Content ??= new StringContent(string.Empty);
            if (!request.Content.Headers.TryAddWithoutValidation(header.Key, header.Value))
            {
                throw new InvalidOperationException($"Unable to add header '{header.Key}' to the request.");
            }
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