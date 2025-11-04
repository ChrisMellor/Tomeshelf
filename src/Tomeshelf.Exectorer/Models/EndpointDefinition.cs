namespace Tomeshelf.Executor.Models;

/// <summary>
///     Declarative definition for an endpoint that can be executed manually or on a schedule.
/// </summary>
public sealed class EndpointDefinition
{
    /// <summary>
    ///     Unique identifier for the endpoint.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    ///     Friendly display name shown in the UI.
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    ///     Optional description providing additional context to the operator.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    ///     HTTP method to use when invoking the endpoint. Defaults to GET.
    /// </summary>
    public string Method { get; set; } = "GET";

    /// <summary>
    ///     Optional named service host (e.g. comicconapi) that should be combined with the configured scheme and path.
    /// </summary>
    public string? Service { get; set; }

    /// <summary>
    ///     Optional fully qualified URL. Takes precedence over Service/Path.
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    ///     Relative path that is appended to the base host. Defaults to "/".
    /// </summary>
    public string? Path { get; set; }

    /// <summary>
    ///     Optional port used when building the base URL for Service.
    /// </summary>
    public int? Port { get; set; }

    /// <summary>
    ///     Default headers applied to each request.
    /// </summary>
    public Dictionary<string, string> Headers { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    ///     Optional request body template, surfaced in the manual execution UI.
    /// </summary>
    public string? BodyTemplate { get; set; }

    /// <summary>
    ///     Optional content type used when serialising the body template (and manual overrides when unspecified).
    /// </summary>
    public string? BodyContentType { get; set; }

    /// <summary>
    ///     Optional CRON expression for automated execution.
    /// </summary>
    public string? Schedule { get; set; }

    /// <summary>
    ///     Optional IANA/Windows time zone identifier used for schedule evaluation.
    ///     Falls back to the application default when omitted.
    /// </summary>
    public string? TimeZone { get; set; }

    /// <summary>
    ///     Optional timeout in seconds. When specified, overrides the default HttpClient timeout.
    /// </summary>
    public int? TimeoutSeconds { get; set; }

    /// <summary>
    ///     Indicates whether the endpoint supports a request body. Defaults based on HTTP method when unspecified.
    /// </summary>
    public bool? AllowBody { get; set; }
}