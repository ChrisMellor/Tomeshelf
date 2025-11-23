using System;

namespace Tomeshelf.Infrastructure.Bundles;

/// <summary>
///     Lightweight representation of a bundle parsed from the Humble Bundle listing page.
/// </summary>
public sealed record ScrapedBundle
{
    /// <summary>
    ///     Lightweight representation of a bundle parsed from the Humble Bundle listing page.
    /// </summary>
    /// <param name="machineName">Stable machine-readable identifier.</param>
    /// <param name="category">Top-level category (books/games/software/etc.).</param>
    /// <param name="stamp">Stamp/type column from the listing.</param>
    /// <param name="title">Full marketing title.</param>
    /// <param name="shortName">Shorter tile name.</param>
    /// <param name="url">Absolute product URL.</param>
    /// <param name="tileImageUrl">Primary tile image URL.</param>
    /// <param name="tileLogoUrl">Tile logo URL (if available).</param>
    /// <param name="heroImageUrl">Hero/banner image URL (if available).</param>
    /// <param name="shortDescription">Short marketing blurb.</param>
    /// <param name="startsAt">Start date/time (UTC) when provided.</param>
    /// <param name="endsAt">End date/time (UTC) when provided.</param>
    /// <param name="observedUtc">When the scraper captured this bundle.</param>
    public ScrapedBundle(string machineName, string category, string stamp, string title, string shortName, string url, string tileImageUrl, string tileLogoUrl,
            string heroImageUrl, string shortDescription, DateTimeOffset? startsAt, DateTimeOffset? endsAt, DateTimeOffset observedUtc)
    {
        MachineName = machineName;
        Category = category;
        Stamp = stamp;
        Title = title;
        ShortName = shortName;
        Url = url;
        TileImageUrl = tileImageUrl;
        TileLogoUrl = tileLogoUrl;
        HeroImageUrl = heroImageUrl;
        ShortDescription = shortDescription;
        StartsAt = startsAt;
        EndsAt = endsAt;
        ObservedUtc = observedUtc;
    }

    /// <summary>Stable machine-readable identifier.</summary>
    public string MachineName { get; init; }

    /// <summary>Top-level category (books/games/software/etc.).</summary>
    public string Category { get; init; }

    /// <summary>Stamp/type column from the listing.</summary>
    public string Stamp { get; init; }

    /// <summary>Full marketing title.</summary>
    public string Title { get; init; }

    /// <summary>Shorter tile name.</summary>
    public string ShortName { get; init; }

    /// <summary>Absolute product URL.</summary>
    public string Url { get; init; }

    /// <summary>Primary tile image URL.</summary>
    public string TileImageUrl { get; init; }

    /// <summary>Tile logo URL (if available).</summary>
    public string TileLogoUrl { get; init; }

    /// <summary>Hero/banner image URL (if available).</summary>
    public string HeroImageUrl { get; init; }

    /// <summary>Short marketing blurb.</summary>
    public string ShortDescription { get; init; }

    /// <summary>Start date/time (UTC) when provided.</summary>
    public DateTimeOffset? StartsAt { get; init; }

    /// <summary>End date/time (UTC) when provided.</summary>
    public DateTimeOffset? EndsAt { get; init; }

    /// <summary>When the scraper captured this bundle.</summary>
    public DateTimeOffset ObservedUtc { get; init; }

    public void Deconstruct(out string machineName, out string category, out string stamp, out string title, out string shortName, out string url, out string tileImageUrl,
            out string tileLogoUrl, out string heroImageUrl, out string shortDescription, out DateTimeOffset? startsAt, out DateTimeOffset? endsAt, out DateTimeOffset observedUtc)
    {
        machineName = MachineName;
        category = Category;
        stamp = Stamp;
        title = Title;
        shortName = ShortName;
        url = Url;
        tileImageUrl = TileImageUrl;
        tileLogoUrl = TileLogoUrl;
        heroImageUrl = HeroImageUrl;
        shortDescription = ShortDescription;
        startsAt = StartsAt;
        endsAt = EndsAt;
        observedUtc = ObservedUtc;
    }
}