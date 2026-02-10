using System;

namespace Tomeshelf.Fitbit.Infrastructure;

internal sealed record FetchResult<T>(T Value, Exception Exception)
{
    public bool IsSuccess => Exception is null;

    /// <summary>
    ///     Failures.
    /// </summary>
    /// <param name="exception">The exception.</param>
    /// <returns>The result of the operation.</returns>
    public static FetchResult<T> Failure(Exception exception)
    {
        return new FetchResult<T>(default, exception);
    }

    /// <summary>
    ///     Success.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The result of the operation.</returns>
    public static FetchResult<T> Success(T value)
    {
        return new FetchResult<T>(value, null);
    }
}