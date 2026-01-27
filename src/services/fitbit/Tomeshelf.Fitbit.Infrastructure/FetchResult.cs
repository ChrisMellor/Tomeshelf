using System;

namespace Tomeshelf.Fitbit.Infrastructure;

internal sealed record FetchResult<T>(T Value, Exception Exception)
{
    public bool IsSuccess => Exception is null;

    public static FetchResult<T> Success(T value) => new(value, null);

    public static FetchResult<T> Failure(Exception exception) => new(default, exception);
}
