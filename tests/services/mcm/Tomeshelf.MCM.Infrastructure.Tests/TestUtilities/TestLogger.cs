using Microsoft.Extensions.Logging;

namespace Tomeshelf.MCM.Infrastructure.Tests.TestUtilities;

public class TestLogger<T> : ILogger<T>
{
    public List<LogEntry> LogEntries { get; } = new List<LogEntry>();

    IDisposable ILogger.BeginScope<TState>(TState state)
    {
        return NullScope.Instance;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        LogEntries.Add(new LogEntry
        {
            LogLevel = logLevel,
            EventId = eventId,
            State = state,
            Exception = exception,
            Message = formatter(state, exception)
        });
    }

    private sealed class NullScope : IDisposable
    {
        public static NullScope Instance { get; } = new NullScope();
        public void Dispose() { }
    }
}

public class LogEntry
{
    public LogLevel LogLevel { get; set; }
    public EventId EventId { get; set; }
    public object? State { get; set; }
    public Exception? Exception { get; set; }
    public string? Message { get; set; }
}
