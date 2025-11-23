namespace Tomeshelf.ServiceDefaults;

/// <summary>
///     Represents a single discovered endpoint.
/// </summary>
public sealed record ExecutorDiscoveredEndpoint
{
    /// <summary>
    ///     Represents a single discovered endpoint.
    /// </summary>
    /// <param name="id">Stable identifier for the endpoint.</param>
    /// <param name="method">HTTP method.</param>
    /// <param name="relativePath">Relative path (leading slash).</param>
    /// <param name="displayName">Optional human-friendly name.</param>
    /// <param name="description">Optional description snippet.</param>
    /// <param name="allowBody">Indicates whether the endpoint supports a request body.</param>
    /// <param name="groupName">Optional group/category name.</param>
    public ExecutorDiscoveredEndpoint(string id, string method, string relativePath, string? displayName, string? description, bool allowBody, string? groupName)
    {
        Id = id;
        Method = method;
        RelativePath = relativePath;
        DisplayName = displayName;
        Description = description;
        AllowBody = allowBody;
        GroupName = groupName;
    }

    /// <summary>Stable identifier for the endpoint.</summary>
    public string Id { get; init; }

    /// <summary>HTTP method.</summary>
    public string Method { get; init; }

    /// <summary>Relative path (leading slash).</summary>
    public string RelativePath { get; init; }

    /// <summary>Optional human-friendly name.</summary>
    public string? DisplayName { get; init; }

    /// <summary>Optional description snippet.</summary>
    public string? Description { get; init; }

    /// <summary>Indicates whether the endpoint supports a request body.</summary>
    public bool AllowBody { get; init; }

    /// <summary>Optional group/category name.</summary>
    public string? GroupName { get; init; }

    public void Deconstruct(out string id, out string method, out string relativePath, out string? displayName, out string? description, out bool allowBody, out string? groupName)
    {
        id = Id;
        method = Method;
        relativePath = RelativePath;
        displayName = DisplayName;
        description = Description;
        allowBody = AllowBody;
        groupName = GroupName;
    }
}