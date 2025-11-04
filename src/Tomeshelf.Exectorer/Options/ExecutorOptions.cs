using Tomeshelf.Executor.Models;

namespace Tomeshelf.Executor.Options;

/// <summary>
///     Configuration options for the Executor application.
/// </summary>
public sealed class ExecutorOptions
{
    /// <summary>
    ///     Configuration section name.
    /// </summary>
    public const string SectionName = "Executor";

    /// <summary>
    ///     Default scheme applied when building service-based URLs outside of Docker.
    /// </summary>
    public string DefaultScheme { get; set; } = "https";

    /// <summary>
    ///     Default scheme applied when building service-based URLs inside Docker.
    /// </summary>
    public string DefaultDockerScheme { get; set; } = "http";

    /// <summary>
    ///     Default timezone (IANA or Windows identifier) used when evaluating cron expressions.
    /// </summary>
    public string DefaultTimeZone { get; set; } = "UTC";

    /// <summary>
    ///     Enables or disables the background scheduler globally.
    /// </summary>
    public bool EnableScheduling { get; set; } = true;

    /// <summary>
    ///     Declared endpoints that can be executed.
    /// </summary>
    public List<EndpointDefinition> Endpoints { get; set; } = new();

    /// <summary>
    ///     Downstream services that should be auto-discovered.
    /// </summary>
    public List<AutoDiscoverySource> AutoDiscovery { get; set; } = new();

    /// <summary>
    ///     Refresh interval for auto-discovery (in seconds). Set to 0 to disable periodic refresh.
    /// </summary>
    public int AutoDiscoveryRefreshIntervalSeconds { get; set; } = 900;
}