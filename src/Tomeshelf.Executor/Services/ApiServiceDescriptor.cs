namespace Tomeshelf.Executor.Services;

/// <summary>
///     Describes a discovered API base address.
/// </summary>
public sealed record ApiServiceDescriptor
{
    /// <summary>
    ///     Describes a discovered API base address.
    /// </summary>
    /// <param name="serviceName">Logical resource name.</param>
    /// <param name="displayName">Friendly label.</param>
    /// <param name="baseAddress">Absolute base URI.</param>
    public ApiServiceDescriptor(string serviceName, string displayName, string baseAddress)
    {
        ServiceName = serviceName;
        DisplayName = displayName;
        BaseAddress = baseAddress;
    }

    /// <summary>Logical resource name.</summary>
    public string ServiceName { get; init; }

    /// <summary>Friendly label.</summary>
    public string DisplayName { get; init; }

    /// <summary>Absolute base URI.</summary>
    public string BaseAddress { get; init; }

    public void Deconstruct(out string serviceName, out string displayName, out string baseAddress)
    {
        serviceName = ServiceName;
        displayName = DisplayName;
        baseAddress = BaseAddress;
    }
}