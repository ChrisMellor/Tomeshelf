using System;

namespace Tomeshelf.Web.Models.Bundles;

/// <summary>
///     View model describing a bundle for the MVC view.
/// </summary>
public sealed class BundleViewModel
{
    public string MachineName { get; init; }

    public string Category { get; init; }

    public string Stamp { get; init; }

    public string Title { get; init; }

    public string ShortName { get; init; }

    public string Url { get; init; }

    public string TileImageUrl { get; init; }

    public string TileLogoUrl { get; init; }

    public string HeroImageUrl { get; init; }

    public string ShortDescription { get; init; }

    public DateTimeOffset? StartsAt { get; init; }

    public DateTimeOffset? EndsAt { get; init; }

    public DateTimeOffset FirstSeenUtc { get; init; }

    public DateTimeOffset LastSeenUtc { get; init; }

    public DateTimeOffset LastUpdatedUtc { get; init; }

    public bool IsExpired { get; init; }

    public TimeSpan? TimeRemaining { get; init; }

    public double? SecondsRemaining { get; init; }
}