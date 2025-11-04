using Quartz;

namespace Tomeshelf.Executor.Models;

/// <summary>
///     Runtime descriptor built from configuration, used to execute outgoing requests.
/// </summary>
internal sealed class EndpointDescriptor
{
    public EndpointDescriptor(string id, string displayName, string method, HttpMethod httpMethod, Uri uri, string? description, IReadOnlyDictionary<string, string> headers, string? bodyTemplate, string? bodyContentType, CronExpression? cronExpression, TimeZoneInfo? timeZone, TimeSpan? timeout, bool allowBody)
    {
        Id = id;
        DisplayName = displayName;
        Method = method;
        HttpMethod = httpMethod;
        Uri = uri;
        Description = description;
        Headers = headers;
        BodyTemplate = bodyTemplate;
        BodyContentType = bodyContentType;
        CronExpression = cronExpression;
        TimeZone = timeZone;
        Timeout = timeout;
        AllowBody = allowBody;
    }

    public string Id { get; }

    public string DisplayName { get; }

    public string Method { get; }

    public HttpMethod HttpMethod { get; }

    public Uri Uri { get; }

    public string? Description { get; }

    public IReadOnlyDictionary<string, string> Headers { get; }

    public string? BodyTemplate { get; }

    public string? BodyContentType { get; }

    public CronExpression? CronExpression { get; }

    public TimeZoneInfo? TimeZone { get; }

    public TimeSpan? Timeout { get; }

    public bool AllowBody { get; }
}