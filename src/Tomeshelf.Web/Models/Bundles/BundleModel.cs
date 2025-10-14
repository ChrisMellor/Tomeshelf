using System;

namespace Tomeshelf.Web.Models.Bundles;

/// <summary>
///     DTO mirrored from the Humble Bundle API response.
/// </summary>
public sealed class BundleModel
{
    public string MachineName { get; set; }

    public string Category { get; set; }

    public string Stamp { get; set; }

    public string Title { get; set; }

    public string ShortName { get; set; }

    public string Url { get; set; }

    public string TileImageUrl { get; set; }

    public string TileLogoUrl { get; set; }

    public string HeroImageUrl { get; set; }

    public string ShortDescription { get; set; }

    public DateTimeOffset? StartsAt { get; set; }

    public DateTimeOffset? EndsAt { get; set; }

    public DateTimeOffset FirstSeenUtc { get; set; }

    public DateTimeOffset LastSeenUtc { get; set; }

    public DateTimeOffset LastUpdatedUtc { get; set; }

    public double? SecondsRemaining { get; set; }

    public DateTimeOffset GeneratedUtc { get; set; }
}