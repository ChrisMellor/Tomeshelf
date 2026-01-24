using System;

namespace Tomeshelf.HumbleBundle.Domain.HumbleBundle;

/// <summary>
///     Represents a Humble Bundle offering captured from the public bundles listing.
/// </summary>
public class Bundle
{
    public int Id { get; set; }

    /// <summary>
    ///     Humble's machine identifier for the bundle (stable key).
    /// </summary>
    public string MachineName { get; set; } = string.Empty;

    /// <summary>
    ///     Broad category (e.g., books, games, software) the bundle belongs to.
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    ///     Stamp/type label from the Humble listing (e.g., bundle, store).
    /// </summary>
    public string Stamp { get; set; } = string.Empty;

    /// <summary>
    ///     Display name presented on the Humble site (full title).
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    ///     Shorter marketing name when available.
    /// </summary>
    public string ShortName { get; set; } = string.Empty;

    /// <summary>
    ///     Absolute product URL that customers can visit to view the bundle.
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    ///     Primary tile image displayed on the listing page.
    /// </summary>
    public string TileImageUrl { get; set; } = string.Empty;

    /// <summary>
    ///     Hero/large banner image when available.
    /// </summary>
    public string HeroImageUrl { get; set; } = string.Empty;

    /// <summary>
    ///     Logo image displayed within the tile, when available.
    /// </summary>
    public string TileLogoUrl { get; set; } = string.Empty;

    /// <summary>
    ///     Short marketing description snippet.
    /// </summary>
    public string ShortDescription { get; set; } = string.Empty;

    /// <summary>
    ///     Timestamp the bundle becomes available (UTC).
    /// </summary>
    public DateTimeOffset? StartsAt { get; set; }

    /// <summary>
    ///     Timestamp the bundle is scheduled to end (UTC).
    /// </summary>
    public DateTimeOffset? EndsAt { get; set; }

    /// <summary>
    ///     First time the scraper observed this bundle.
    /// </summary>
    public DateTimeOffset FirstSeenUtc { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    ///     Last time the scraper saw this bundle in the listings.
    /// </summary>
    public DateTimeOffset LastSeenUtc { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    ///     Last time the metadata was updated based on scraped data.
    /// </summary>
    public DateTimeOffset LastUpdatedUtc { get; set; } = DateTimeOffset.UtcNow;
}