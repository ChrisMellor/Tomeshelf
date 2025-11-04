using System.ComponentModel.DataAnnotations;
using Tomeshelf.ServiceDefaults;

namespace Tomeshelf.Executor.Options;

/// <summary>
///     Configuration for automatic endpoint discovery from downstream APIs.
/// </summary>
public sealed class AutoDiscoverySource
{
    /// <summary>
    ///     Logical service name used for host resolution (e.g. comicconapi).
    /// </summary>
    [Required]
    public string Service { get; set; } = string.Empty;

    /// <summary>
    ///     Optional explicit base URL override. When specified, Service/Scheme/Port are ignored.
    /// </summary>
    public string? BaseUrl { get; set; }

    /// <summary>
    ///     Optional scheme override (defaults to Executor defaults).
    /// </summary>
    public string? Scheme { get; set; }

    /// <summary>
    ///     Optional port override used when composing service-based URLs.
    /// </summary>
    public int? Port { get; set; }

    /// <summary>
    ///     Discovery endpoint path.
    /// </summary>
    public string DiscoveryPath { get; set; } = ExecutorDiscoveryConstants.DefaultPath;

    /// <summary>
    ///     Optional prefix applied to generated display names.
    /// </summary>
    public string? DisplayPrefix { get; set; }

    /// <summary>
    ///     Optional schedule applied to all discovered endpoints (defaults to none).
    /// </summary>
    public string? Schedule { get; set; }

    /// <summary>
    ///     Optional timezone identifier used for scheduled endpoints.
    /// </summary>
    public string? TimeZone { get; set; }
}