using Microsoft.Extensions.Options;

namespace Tomeshelf.SHiFT.Infrastructure.Tests.TestUtilities;

public sealed class TestOptionsMonitor<T> : IOptionsMonitor<T>
{
    private readonly List<Action<T, string>> _listeners = new List<Action<T, string>>();

    /// <summary>
    ///     Initializes a new instance of the <see cref="TestOptionsMonitor" /> class.
    /// </summary>
    /// <param name="currentValue">The current value.</param>
    public TestOptionsMonitor(T currentValue)
    {
        CurrentValue = currentValue;
    }

    public T CurrentValue { get; private set; }

    /// <summary>
    ///     Gets.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <returns>The result of the operation.</returns>
    public T Get(string? name)
    {
        return CurrentValue;
    }

    /// <summary>
    ///     Ons the change.
    /// </summary>
    /// <param name="listener">The listener.</param>
    /// <returns>The result of the operation.</returns>
    public IDisposable OnChange(Action<T, string> listener)
    {
        _listeners.Add(listener);

        return new ActionDisposable(() => _listeners.Remove(listener));
    }

    /// <summary>
    ///     Sets.
    /// </summary>
    /// <param name="value">The value.</param>
    public void Set(T value)
    {
        CurrentValue = value;
        foreach (var listener in _listeners.ToArray())
        {
            listener(value, string.Empty);
        }
    }

    private sealed class ActionDisposable(Action dispose) : IDisposable
    {
        private readonly Action _dispose = dispose;
        private bool _disposed;

        /// <summary>
        ///     Releases resources used by this instance.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _dispose();
        }
    }
}
