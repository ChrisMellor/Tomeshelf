using Microsoft.Extensions.Logging;

namespace Tomeshelf.MCM.Infrastructure.Tests.TestUtilities;

public class TestLogger<T> : ILogger<T>
{
    public List<LogEntry> LogEntries { get; } = new();

    /// <summary>
    ///     Begins the scope.
    /// </summary>
    /// <typeparam name="TState">The type of TState.</typeparam>
    /// <param name="state">The state.</param>
    /// <returns>The result of the operation.</returns>
    IDisposable ILogger.BeginScope<TState>(TState state)
    {
        return NullScope.Instance;
    }

    /// <summary>
    ///     Determines whether the specified log level is enabled.
    /// </summary>
    /// <param name="logLevel">The log level.</param>
    /// <returns>True if the condition is met; otherwise, false.</returns>
    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    /// <summary>
    ///     Logs.
    /// </summary>
    /// <typeparam name="TState">The type of TState.</typeparam>
    /// <param name="logLevel">The log level.</param>
    /// <param name="eventId">The event id.</param>
    /// <param name="state">The state.</param>
    /// <param name="exception">The exception.</param>
    /// <param name="formatter">The formatter.</param>
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
        public static NullScope Instance { get; } = new();

        /// <summary>
        ///     Releases resources used by this instance.
        /// </summary>
        public void Dispose() { }
    }
}