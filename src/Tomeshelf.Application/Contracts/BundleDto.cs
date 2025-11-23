using System;

namespace Tomeshelf.Application.Contracts;

/// <summary>
///     Read model returned by the Humble Bundle API describing a bundle listing.
/// </summary>
public sealed record BundleDto
{
    /// <summary>
    ///     Read model returned by the Humble Bundle API describing a bundle listing.
    /// </summary>
    public BundleDto(string machineName, string category, string stamp, string title, string shortName, string url, string tileImageUrl, string tileLogoUrl, string heroImageUrl,
            string shortDescription, DateTimeOffset? startsAt, DateTimeOffset? endsAt, DateTimeOffset firstSeenUtc, DateTimeOffset lastSeenUtc, DateTimeOffset lastUpdatedUtc,
            DateTimeOffset generatedUtc)
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
        FirstSeenUtc = firstSeenUtc;
        LastSeenUtc = lastSeenUtc;
        LastUpdatedUtc = lastUpdatedUtc;
        GeneratedUtc = generatedUtc;
    }

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

    public DateTimeOffset GeneratedUtc { get; init; }

    public void Deconstruct(out string machineName, out string category, out string stamp, out string title, out string shortName, out string url, out string tileImageUrl,
            out string tileLogoUrl, out string heroImageUrl, out string shortDescription, out DateTimeOffset? startsAt, out DateTimeOffset? endsAt, out DateTimeOffset firstSeenUtc,
            out DateTimeOffset lastSeenUtc, out DateTimeOffset lastUpdatedUtc, out DateTimeOffset generatedUtc)
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
        firstSeenUtc = FirstSeenUtc;
        lastSeenUtc = LastSeenUtc;
        lastUpdatedUtc = LastUpdatedUtc;
        generatedUtc = GeneratedUtc;
    }
}