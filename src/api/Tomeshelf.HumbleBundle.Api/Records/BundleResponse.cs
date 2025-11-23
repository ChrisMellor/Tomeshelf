using System;
using Tomeshelf.Application.Contracts;

namespace Tomeshelf.HumbleBundle.Api.Records;

/// <summary>
///     API model returned by the GET endpoint, enriched with computed remaining time.
/// </summary>
public sealed record BundleResponse
{
    /// <summary>
    ///     API model returned by the GET endpoint, enriched with computed remaining time.
    /// </summary>
    /// <param name="machineName">Stable identifier.</param>
    /// <param name="category">Bundle category.</param>
    /// <param name="stamp">Bundle type stamp.</param>
    /// <param name="title">Full display title.</param>
    /// <param name="shortName">Short marketing title.</param>
    /// <param name="url">Product URL.</param>
    /// <param name="tileImageUrl">Tile image.</param>
    /// <param name="tileLogoUrl">Tile logo.</param>
    /// <param name="heroImageUrl">Hero image.</param>
    /// <param name="shortDescription">Short description text.</param>
    /// <param name="startsAt">Start date.</param>
    /// <param name="endsAt">End date.</param>
    /// <param name="firstSeenUtc">First time the scraper observed the bundle.</param>
    /// <param name="lastSeenUtc">Last time the bundle was observed.</param>
    /// <param name="lastUpdatedUtc">Last time metadata changed.</param>
    /// <param name="secondsRemaining">Seconds remaining until expiry, when applicable.</param>
    /// <param name="generatedUtc">Timestamp when this projection was generated.</param>
    public BundleResponse(string machineName, string category, string stamp, string title, string shortName, string url, string tileImageUrl, string tileLogoUrl,
            string heroImageUrl, string shortDescription, DateTimeOffset? startsAt, DateTimeOffset? endsAt, DateTimeOffset firstSeenUtc, DateTimeOffset lastSeenUtc,
            DateTimeOffset lastUpdatedUtc, double? secondsRemaining, DateTimeOffset generatedUtc)
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
        SecondsRemaining = secondsRemaining;
        GeneratedUtc = generatedUtc;
    }

    /// <summary>Stable identifier.</summary>
    public string MachineName { get; init; }

    /// <summary>Bundle category.</summary>
    public string Category { get; init; }

    /// <summary>Bundle type stamp.</summary>
    public string Stamp { get; init; }

    /// <summary>Full display title.</summary>
    public string Title { get; init; }

    /// <summary>Short marketing title.</summary>
    public string ShortName { get; init; }

    /// <summary>Product URL.</summary>
    public string Url { get; init; }

    /// <summary>Tile image.</summary>
    public string TileImageUrl { get; init; }

    /// <summary>Tile logo.</summary>
    public string TileLogoUrl { get; init; }

    /// <summary>Hero image.</summary>
    public string HeroImageUrl { get; init; }

    /// <summary>Short description text.</summary>
    public string ShortDescription { get; init; }

    /// <summary>Start date.</summary>
    public DateTimeOffset? StartsAt { get; init; }

    /// <summary>End date.</summary>
    public DateTimeOffset? EndsAt { get; init; }

    /// <summary>First time the scraper observed the bundle.</summary>
    public DateTimeOffset FirstSeenUtc { get; init; }

    /// <summary>Last time the bundle was observed.</summary>
    public DateTimeOffset LastSeenUtc { get; init; }

    /// <summary>Last time metadata changed.</summary>
    public DateTimeOffset LastUpdatedUtc { get; init; }

    /// <summary>Seconds remaining until expiry, when applicable.</summary>
    public double? SecondsRemaining { get; init; }

    /// <summary>Timestamp when this projection was generated.</summary>
    public DateTimeOffset GeneratedUtc { get; init; }

    public static BundleResponse FromDto(BundleDto dto, DateTimeOffset now)
    {
        double? secondsRemaining = null;
        if (dto.EndsAt.HasValue)
        {
            var remaining = dto.EndsAt.Value - now;
            if (remaining > TimeSpan.Zero)
            {
                secondsRemaining = remaining.TotalSeconds;
            }
        }

        return new BundleResponse(dto.MachineName, dto.Category, dto.Stamp, dto.Title, dto.ShortName, dto.Url, dto.TileImageUrl, dto.TileLogoUrl, dto.HeroImageUrl,
                                  dto.ShortDescription, dto.StartsAt, dto.EndsAt, dto.FirstSeenUtc, dto.LastSeenUtc, dto.LastUpdatedUtc, secondsRemaining, dto.GeneratedUtc);
    }

    public void Deconstruct(out string machineName, out string category, out string stamp, out string title, out string shortName, out string url, out string tileImageUrl,
            out string tileLogoUrl, out string heroImageUrl, out string shortDescription, out DateTimeOffset? startsAt, out DateTimeOffset? endsAt, out DateTimeOffset firstSeenUtc,
            out DateTimeOffset lastSeenUtc, out DateTimeOffset lastUpdatedUtc, out double? secondsRemaining, out DateTimeOffset generatedUtc)
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
        secondsRemaining = SecondsRemaining;
        generatedUtc = GeneratedUtc;
    }
}