namespace Tomeshelf.Executor.Models;

/// <summary>
///     Lightweight view model returned to the frontend.
/// </summary>
public sealed record EndpointSummary(string Id, string DisplayName, string Method, string Target, string? Description, string? Schedule, string? TimeZone, string? BodyTemplate, string? BodyContentType, int? TimeoutSeconds, bool AllowBody, string Origin);