using System;

namespace Tomeshelf.HumbleBundle.Application.Features.Bundles.Models;

public sealed record ScrapedBundle(string MachineName, string Category, string Stamp, string Title, string ShortName, string Url, string TileImageUrl, string TileLogoUrl, string HeroImageUrl, string ShortDescription, DateTimeOffset? StartsAt, DateTimeOffset? EndsAt, DateTimeOffset ObservedUtc);