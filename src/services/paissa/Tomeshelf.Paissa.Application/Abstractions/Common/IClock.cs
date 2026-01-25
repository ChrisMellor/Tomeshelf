using System;

namespace Tomeshelf.Paissa.Application.Abstractions.Common;

/// <summary>
///     Provides the current time in UTC.
/// </summary>
public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
