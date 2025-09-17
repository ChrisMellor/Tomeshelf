using System;
using System.Collections.Generic;
using Tomeshelf.Infrastructure.Queries;

namespace Tomeshelf.Api.Services;

/// <summary>
/// In-memory cache for guests-by-city snapshots served by the API.
/// </summary>
public interface IGuestsCache
{
    bool TryGet(string city, out GuestsSnapshot snapshot);

    void Set(string city, GuestsSnapshot snapshot);

    void Remove(string city);
}

/// <summary>
/// Immutable snapshot of guests data for a city.
/// </summary>
/// <param name="City">City key for the snapshot.</param>
/// <param name="Total">Total number of guests across all groups.</param>
/// <param name="Groups">Date-grouped results.</param>
/// <param name="GeneratedUtc">When the snapshot was generated (UTC).</param>
public sealed record GuestsSnapshot(
    string City,
    int Total,
    IReadOnlyList<GuestQueries.GuestsGroupResult> Groups,
    DateTimeOffset GeneratedUtc
);

