using Tomeshelf.ComicCon.Api.Records;

namespace Tomeshelf.ComicCon.Api.Services;

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
}