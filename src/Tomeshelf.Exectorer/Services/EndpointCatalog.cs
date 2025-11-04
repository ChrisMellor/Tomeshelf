using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using Microsoft.Extensions.Options;
using Quartz;
using Tomeshelf.Executor.Helpers;
using Tomeshelf.Executor.Models;
using Tomeshelf.Executor.Options;

namespace Tomeshelf.Executor.Services;

/// <summary>
///     Materialises endpoint descriptors from configuration and discovery sources.
/// </summary>
public sealed class EndpointCatalog
{
    private const string ManualOrigin = "configuration";
    private readonly string _defaultTimeZoneId;

    private readonly ConcurrentDictionary<string, EndpointDescriptor> _discoveredDescriptors = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, HashSet<string>> _discoveredIndex = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, EndpointSummary> _discoveredSummaries = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _discoveryLock = new();
    private readonly string _dockerScheme;

    private readonly bool _isDocker;
    private readonly ILogger<EndpointCatalog> _logger;

    private readonly Dictionary<string, EndpointDescriptor> _manualDescriptors = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<EndpointSummary> _manualSummaries = new();
    private readonly string _primaryScheme;

    public EndpointCatalog(IOptions<ExecutorOptions> options, ILogger<EndpointCatalog> logger)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);

        _logger = logger;

        var configuration = options.Value ?? new ExecutorOptions();
        _isDocker = RuntimeEnvironment.IsRunningInDocker();
        _primaryScheme = string.IsNullOrWhiteSpace(configuration.DefaultScheme)
                ? "https"
                : configuration.DefaultScheme.Trim();
        _dockerScheme = string.IsNullOrWhiteSpace(configuration.DefaultDockerScheme)
                ? "http"
                : configuration.DefaultDockerScheme.Trim();
        _defaultTimeZoneId = string.IsNullOrWhiteSpace(configuration.DefaultTimeZone)
                ? "UTC"
                : configuration.DefaultTimeZone.Trim();

        foreach (var definition in configuration.Endpoints ?? Enumerable.Empty<EndpointDefinition>())
        {
            if (definition is null)
            {
                continue;
            }

            var materialised = TryBuildDescriptor(definition, ManualOrigin, true);
            if (materialised is null)
            {
                continue;
            }

            var (descriptor, summary) = materialised.Value;

            if (_manualDescriptors.ContainsKey(descriptor.Id))
            {
                throw new InvalidOperationException($"Duplicate executor endpoint id detected: '{descriptor.Id}'. Each endpoint must be unique.");
            }

            _manualDescriptors.Add(descriptor.Id, descriptor);
            _manualSummaries.Add(summary);
        }
    }

    /// <summary>
    ///     Retrieves all configured endpoints in summary form.
    /// </summary>
    public IReadOnlyList<EndpointSummary> GetSummaries()
    {
        var combined = new List<EndpointSummary>(_manualSummaries.Count + _discoveredSummaries.Count);
        combined.AddRange(_manualSummaries);
        combined.AddRange(_discoveredSummaries.Values);

        return combined.OrderBy(summary => summary.DisplayName, StringComparer.OrdinalIgnoreCase)
                       .ThenBy(summary => summary.Id, StringComparer.OrdinalIgnoreCase)
                       .ToList();
    }

    /// <summary>
    ///     Returns the full descriptor set for internal consumers (e.g. scheduler).
    /// </summary>
    internal IEnumerable<EndpointDescriptor> GetDescriptors()
    {
        return _manualDescriptors.Values.Concat(_discoveredDescriptors.Values);
    }

    internal bool TryGetDescriptor(string id, out EndpointDescriptor descriptor)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        if (_manualDescriptors.TryGetValue(id, out descriptor!))
        {
            return true;
        }

        return _discoveredDescriptors.TryGetValue(id, out descriptor!);
    }

    /// <summary>
    ///     Replaces discovered endpoints for a given source key.
    /// </summary>
    internal void ReplaceDiscoveredEndpoints(string sourceKey, IEnumerable<EndpointDefinition> definitions)
    {
        if (string.IsNullOrWhiteSpace(sourceKey))
        {
            throw new ArgumentException("Source key must be provided.", nameof(sourceKey));
        }

        var originLabel = $"discovery:{sourceKey}";
        var replacements = new List<(EndpointDescriptor Descriptor, EndpointSummary Summary)>();

        foreach (var definition in definitions ?? Enumerable.Empty<EndpointDefinition>())
        {
            if (definition is null)
            {
                continue;
            }

            var materialised = TryBuildDescriptor(definition, originLabel, false);
            if (materialised is null)
            {
                continue;
            }

            var (descriptor, summary) = materialised.Value;

            if (_manualDescriptors.ContainsKey(descriptor.Id))
            {
                _logger.LogDebug("Skipping discovered endpoint {EndpointId} from {Source} because a manual definition exists.", descriptor.Id, sourceKey);

                continue;
            }

            replacements.Add(materialised.Value);
        }

        lock (_discoveryLock)
        {
            if (_discoveredIndex.TryGetValue(sourceKey, out var previous))
            {
                foreach (var id in previous)
                {
                    _discoveredDescriptors.TryRemove(id, out _);
                    _discoveredSummaries.TryRemove(id, out _);
                }
            }

            var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var (descriptor, summary) in replacements)
            {
                _discoveredDescriptors[descriptor.Id] = descriptor;
                _discoveredSummaries[summary.Id] = summary;
                ids.Add(descriptor.Id);
            }

            _discoveredIndex[sourceKey] = ids;
        }
    }

    private (EndpointDescriptor Descriptor, EndpointSummary Summary)? TryBuildDescriptor(EndpointDefinition definition, string originLabel, bool throwOnError)
    {
        try
        {
            var id = definition.Id?.Trim();
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new InvalidOperationException("Executor endpoint definitions require a non-empty 'Id'.");
            }

            var schedule = string.IsNullOrWhiteSpace(definition.Schedule)
                    ? null
                    : definition.Schedule.Trim();
            var methodString = HttpMethodUtilities.Normalise(definition.Method);
            var httpMethod = HttpMethodUtilities.Resolve(methodString);
            var uri = ResolveEndpointUri(definition, _primaryScheme, _dockerScheme, _isDocker);
            var headers = BuildHeaders(definition.Headers);
            var bodyTemplate = definition.BodyTemplate;
            var bodyContentType = ResolveBodyContentType(definition);
            var timeZone = ResolveTimeZone(definition.TimeZone, _defaultTimeZoneId, schedule is not null, id);
            var cronExpression = ParseCronExpression(schedule, id, timeZone);
            var timeout = ResolveTimeout(definition.TimeoutSeconds, id);
            var allowBody = DetermineAllowBody(definition.AllowBody, methodString, bodyTemplate);
            var displayName = string.IsNullOrWhiteSpace(definition.DisplayName)
                    ? id
                    : definition.DisplayName!.Trim();

            var descriptor = new EndpointDescriptor(id, displayName, methodString, httpMethod, uri, definition.Description, headers, bodyTemplate, bodyContentType, cronExpression, timeZone, timeout, allowBody);

            var summary = new EndpointSummary(id, descriptor.DisplayName, descriptor.Method, uri.ToString(), descriptor.Description, schedule, timeZone?.Id, bodyTemplate, bodyContentType, timeout.HasValue
                                                      ? Convert.ToInt32(timeout.Value.TotalSeconds, CultureInfo.InvariantCulture)
                                                      : null, allowBody, originLabel);

            return (descriptor, summary);
        }
        catch (Exception ex) when (!throwOnError)
        {
            _logger.LogWarning(ex, "Failed to materialise discovered endpoint for origin {Origin}.", originLabel);

            return null;
        }
    }

    private static IReadOnlyDictionary<string, string> BuildHeaders(IDictionary<string, string>? headers)
    {
        if (headers is null || (headers.Count == 0))
        {
            return new ReadOnlyDictionary<string, string>(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));
        }

        var values = new Dictionary<string, string>(headers.Count, StringComparer.OrdinalIgnoreCase);

        foreach (var pair in headers)
        {
            if (string.IsNullOrWhiteSpace(pair.Key))
            {
                continue;
            }

            values[pair.Key.Trim()] = pair.Value?.Trim() ?? string.Empty;
        }

        return new ReadOnlyDictionary<string, string>(values);
    }

    private static string? ResolveBodyContentType(EndpointDefinition definition)
    {
        if (!string.IsNullOrWhiteSpace(definition.BodyContentType))
        {
            return definition.BodyContentType.Trim();
        }

        if (!string.IsNullOrWhiteSpace(definition.BodyTemplate))
        {
            return "application/json";
        }

        return null;
    }

    private static bool DetermineAllowBody(bool? configuredValue, string method, string? bodyTemplate)
    {
        if (configuredValue.HasValue)
        {
            return configuredValue.Value;
        }

        if (!string.IsNullOrWhiteSpace(bodyTemplate))
        {
            return true;
        }

        return method switch
        {
                "GET" => false,
                "DELETE" => false,
                "HEAD" => false,
                "OPTIONS" => false,
                _ => true
        };
    }

    private static CronExpression? ParseCronExpression(string? schedule, string id, TimeZoneInfo? timeZone)
    {
        if (string.IsNullOrWhiteSpace(schedule))
        {
            return null;
        }

        var trimmed = schedule.Trim();
        var segments = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var normalized = segments.Length switch
        {
                5 => $"0 {trimmed}",
                6 => trimmed,
                7 => trimmed,
                _ => throw new InvalidOperationException($"Endpoint '{id}' has an invalid CRON expression '{trimmed}'. Expected 5, 6 or 7 segments but received {segments.Length}.")
        };

        try
        {
            var cron = new CronExpression(normalized) { TimeZone = timeZone ?? TimeZoneInfo.Utc };

            return cron;
        }
        catch (FormatException ex)
        {
            throw new InvalidOperationException($"Endpoint '{id}' has an invalid CRON expression '{trimmed}'.", ex);
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException)
        {
            throw new InvalidOperationException($"Endpoint '{id}' has an invalid CRON expression '{trimmed}'.", ex);
        }
    }

    private static TimeZoneInfo? ResolveTimeZone(string? requested, string fallbackId, bool hasSchedule, string id)
    {
        if (!hasSchedule)
        {
            return null;
        }

        var target = string.IsNullOrWhiteSpace(requested)
                ? fallbackId
                : requested.Trim();

        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(target);
        }
        catch (TimeZoneNotFoundException ex)
        {
            throw new InvalidOperationException($"Endpoint '{id}' references unknown time zone '{target}'.", ex);
        }
        catch (InvalidTimeZoneException ex)
        {
            throw new InvalidOperationException($"Endpoint '{id}' references invalid time zone '{target}'.", ex);
        }
    }

    private static TimeSpan? ResolveTimeout(int? timeoutSeconds, string id)
    {
        if (!timeoutSeconds.HasValue)
        {
            return null;
        }

        if (timeoutSeconds.Value <= 0)
        {
            throw new InvalidOperationException($"Endpoint '{id}' specifies a non-positive timeout '{timeoutSeconds.Value}'.");
        }

        return TimeSpan.FromSeconds(timeoutSeconds.Value);
    }

    private static Uri ResolveEndpointUri(EndpointDefinition definition, string primaryScheme, string dockerScheme, bool isDocker)
    {
        if (!string.IsNullOrWhiteSpace(definition.Url))
        {
            if (!Uri.TryCreate(definition.Url.Trim(), UriKind.Absolute, out var absolute))
            {
                throw new InvalidOperationException($"Endpoint '{definition.Id}' has an invalid Url '{definition.Url}'.");
            }

            return absolute;
        }

        if (string.IsNullOrWhiteSpace(definition.Service))
        {
            throw new InvalidOperationException($"Endpoint '{definition.Id}' must provide either a 'Url' or a 'Service'.");
        }

        var scheme = isDocker
                ? dockerScheme
                : primaryScheme;
        if (string.IsNullOrWhiteSpace(scheme))
        {
            scheme = isDocker
                    ? "http"
                    : "https";
        }

        var builder = new StringBuilder();
        builder.Append(scheme);
        builder.Append("://");
        builder.Append(definition.Service.Trim());

        if (definition.Port.HasValue)
        {
            builder.Append(':');
            builder.Append(definition.Port.Value.ToString(CultureInfo.InvariantCulture));
        }

        if (!Uri.TryCreate(builder.ToString(), UriKind.Absolute, out var baseUri))
        {
            throw new InvalidOperationException($"Endpoint '{definition.Id}' could not create a valid base URI from service '{definition.Service}'.");
        }

        var path = string.IsNullOrWhiteSpace(definition.Path)
                ? "/"
                : definition.Path;

        if (!Uri.TryCreate(baseUri, path, out var resolved))
        {
            throw new InvalidOperationException($"Endpoint '{definition.Id}' produced an invalid path '{definition.Path}'.");
        }

        return resolved;
    }
}