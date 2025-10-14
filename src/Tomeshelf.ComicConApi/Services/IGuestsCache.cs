using System;
using System.Collections.Generic;
using Tomeshelf.Infrastructure.Queries;

namespace Tomeshelf.ComicConApi.Services;

/// <summary>
///     In-memory cache for guests-by-city snapshots served by the API.
/// </summary>
public interface IGuestsCache
{
    /// <summary>
    ///     Attempts to retrieve a cached snapshot for the specified city.
    /// </summary>
    /// <param name="city">City name key.</param>
    /// <param name="snapshot">When successful, receives the cached snapshot.</param>
    /// <returns><c>true</c> when a snapshot exists; otherwise, <c>false</c>.</returns>
    bool TryGet(string city, out GuestsSnapshot snapshot);

    /// <summary>
    ///     Stores or replaces the cached snapshot for the provided city.
    /// </summary>
    /// <param name="city">City name key.</param>
    /// <param name="snapshot">Snapshot to cache.</param>
    void Set(string city, GuestsSnapshot snapshot);

    /// <summary>
    ///     Removes any cached snapshot for the provided city.
    /// </summary>
    /// <param name="city">City name key.</param>
    void Remove(string city);
}

/// <summary>
///     Immutable snapshot of guests data for a city.
/// </summary>
/// <param name="City">City key for the snapshot.</param>
/// <param name="Total">Total number of guests across all groups.</param>
/// <param name="Groups">Date-grouped results.</param>
/// <param name="GeneratedUtc">When the snapshot was generated (UTC).</param>
public sealed record GuestsSnapshot(string City, int Total, IReadOnlyList<GuestQueries.GuestsGroupResult> Groups, DateTimeOffset GeneratedUtc);