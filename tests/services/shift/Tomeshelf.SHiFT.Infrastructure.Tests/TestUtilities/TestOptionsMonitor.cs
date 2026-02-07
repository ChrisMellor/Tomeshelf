using Microsoft.Extensions.Options;

namespace Tomeshelf.SHiFT.Infrastructure.Tests.TestUtilities;

public sealed class TestOptionsMonitor<T> : IOptionsMonitor<T>
{
    private readonly List<Action<T, string>> _listeners = new List<Action<T, string>>();

    public TestOptionsMonitor(T currentValue)
    {
        CurrentValue = currentValue;
    }

    public T CurrentValue { get; private set; }

    public T Get(string? name)
    {
        return CurrentValue;
    }

    public IDisposable OnChange(Action<T, string> listener)
    {
        _listeners.Add(listener);

        return new ActionDisposable(() => _listeners.Remove(listener));
    }

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
