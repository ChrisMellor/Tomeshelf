using System;
using System.Collections.Generic;
using Microsoft.Extensions.Options;

namespace Tomeshelf.Executor.Tests.TestUtilities;

public sealed class TestOptionsMonitor<T> : IOptionsMonitor<T>
{
    private readonly List<Action<T, string>> _listeners = new();
    private T _currentValue;

    public TestOptionsMonitor(T currentValue)
    {
        _currentValue = currentValue;
    }

    public T CurrentValue => _currentValue;

    public T Get(string? name) => _currentValue;

    public IDisposable OnChange(Action<T, string> listener)
    {
        _listeners.Add(listener);
        return new ActionDisposable(() => _listeners.Remove(listener));
    }

    public void Set(T value)
    {
        _currentValue = value;
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
