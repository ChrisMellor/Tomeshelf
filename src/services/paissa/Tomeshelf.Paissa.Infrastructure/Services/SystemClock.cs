using System;
using Tomeshelf.Paissa.Application.Abstractions.Common;

namespace Tomeshelf.Paissa.Infrastructure.Services;

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
