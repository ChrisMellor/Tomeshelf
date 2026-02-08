namespace Tomeshelf.ServiceDefaults;

/// <summary>
///     Represents a single discovered endpoint.
/// </summary>
/// <param name="Id">Stable identifier for the endpoint.</param>
/// <param name="Method">HTTP method.</param>
/// <param name="RelativePath">Relative path (leading slash).</param>
/// <param name="DisplayName">Optional human-friendly name.</param>
/// <param name="Description">Optional description snippet.</param>
/// <param name="AllowBody">Indicates whether the endpoint supports a request body.</param>
/// <param name="GroupName">Optional group/category name.</param>
public sealed record ExecutorDiscoveredEndpoint(string Id, string Method, string RelativePath, string? DisplayName, string? Description, bool AllowBody, string? GroupName);