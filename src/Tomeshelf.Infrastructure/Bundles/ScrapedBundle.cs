using System;

namespace Tomeshelf.Infrastructure.Bundles;

/// <summary>
///     Lightweight representation of a bundle parsed from the Humble Bundle listing page.
/// </summary>
/// <param name="MachineName">Stable machine-readable identifier.</param>
/// <param name="Category">Top-level category (books/games/software/etc.).</param>
/// <param name="Stamp">Stamp/type column from the listing.</param>
/// <param name="Title">Full marketing title.</param>
/// <param name="ShortName">Shorter tile name.</param>
/// <param name="Url">Absolute product URL.</param>
/// <param name="TileImageUrl">Primary tile image URL.</param>
/// <param name="TileLogoUrl">Tile logo URL (if available).</param>
/// <param name="HeroImageUrl">Hero/banner image URL (if available).</param>
/// <param name="ShortDescription">Short marketing blurb.</param>
/// <param name="StartsAt">Start date/time (UTC) when provided.</param>
/// <param name="EndsAt">End date/time (UTC) when provided.</param>
/// <param name="ObservedUtc">When the scraper captured this bundle.</param>
public sealed record ScrapedBundle(string MachineName, string Category, string Stamp, string Title, string ShortName, string Url, string TileImageUrl, string TileLogoUrl, string HeroImageUrl, string ShortDescription, DateTimeOffset? StartsAt, DateTimeOffset? EndsAt, DateTimeOffset ObservedUtc);