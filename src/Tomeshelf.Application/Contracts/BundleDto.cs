using System;

namespace Tomeshelf.Application.Contracts;

/// <summary>
/// Read model returned by the Humble Bundle API describing a bundle listing.
/// </summary>
public sealed record BundleDto(
    string MachineName,
    string Category,
    string Stamp,
    string Title,
    string ShortName,
    string Url,
    string TileImageUrl,
    string TileLogoUrl,
    string HeroImageUrl,
    string ShortDescription,
    DateTimeOffset? StartsAt,
    DateTimeOffset? EndsAt,
    DateTimeOffset FirstSeenUtc,
    DateTimeOffset LastSeenUtc,
    DateTimeOffset LastUpdatedUtc,
    DateTimeOffset GeneratedUtc
);
