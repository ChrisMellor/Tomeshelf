using System;
using System.Collections.Concurrent;

namespace Tomeshelf.ComicCon.Api.Services;

/// <summary>
///     Thread-safe in-memory implementation of <see cref="IGuestsCache" />.
/// </summary>
public sealed class GuestsCache : IGuestsCache
{
    private readonly ConcurrentDictionary<string, GuestsSnapshot> _cache = new(StringComparer.OrdinalIgnoreCase);

    public bool TryGet(string city, out GuestsSnapshot snapshot)
    {
        return _cache.TryGetValue(city, out snapshot!);
    }

    public void Set(string city, GuestsSnapshot snapshot)
    {
        _cache[city] = snapshot;
    }

    public void Remove(string city)
    {
        _cache.TryRemove(city, out _);
    }
}