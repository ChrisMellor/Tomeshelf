using System;

namespace Tomeshelf.Fitbit.Infrastructure;

internal sealed record FetchResult<T>(T Value, Exception Exception)
{
    public bool IsSuccess => Exception is null;

    public static FetchResult<T> Failure(Exception exception)
    {
        return new FetchResult<T>(default, exception);
    }

    public static FetchResult<T> Success(T value)
    {
        return new FetchResult<T>(value, null);
    }
}