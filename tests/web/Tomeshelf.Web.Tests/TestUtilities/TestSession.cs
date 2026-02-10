using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Tomeshelf.Web.Tests.TestUtilities;

public sealed class TestSession : ISession
{
    private readonly Dictionary<string, byte[]> _store = new Dictionary<string, byte[]>(StringComparer.Ordinal);

    /// <summary>
    ///     Clears.
    /// </summary>
    public void Clear()
    {
        _store.Clear();
    }

    /// <summary>
    ///     Commits asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task CommitAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public string Id { get; } = Guid.NewGuid()
                                    .ToString("N");

    public bool IsAvailable { get; set; } = true;

    public IEnumerable<string> Keys => _store.Keys;

    /// <summary>
    ///     Loads asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task LoadAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Removes.
    /// </summary>
    /// <param name="key">The key.</param>
    public void Remove(string key)
    {
        _store.Remove(key);
    }

    /// <summary>
    ///     Sets.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    public void Set(string key, byte[] value)
    {
        _store[key] = value;
    }

    /// <summary>
    ///     Attempts to get a value.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    /// <returns>True if the condition is met; otherwise, false.</returns>
    public bool TryGetValue(string key, out byte[] value)
    {
        return _store.TryGetValue(key, out value);
    }
}