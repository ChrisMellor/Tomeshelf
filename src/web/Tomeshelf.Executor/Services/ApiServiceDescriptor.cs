namespace Tomeshelf.Executor.Services;

/// <summary>
///     Describes a discovered API base address.
/// </summary>
/// <param name="ServiceName">Logical resource name.</param>
/// <param name="DisplayName">Friendly label.</param>
/// <param name="BaseAddress">Absolute base URI.</param>
public sealed record ApiServiceDescriptor(string ServiceName, string DisplayName, string BaseAddress);