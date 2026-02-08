using System;

namespace Tomeshelf.SHiFT.Application.Abstractions.Common;

/// <summary>
///     Provides an abstraction for retrieving the current date and time in Coordinated Universal Time (UTC).
/// </summary>
/// <remarks>
///     Implementations of this interface can be used to obtain a consistent and testable source of UTC time,
///     which is useful for scenarios such as logging, scheduling, or time-based calculations. This abstraction enables
///     easier unit testing by allowing the time source to be substituted or mocked.
/// </remarks>
public interface IClock
{
    DateTimeOffset UtcNow { get; }
}