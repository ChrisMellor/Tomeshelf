using System;
using Tomeshelf.SHiFT.Application.Abstractions.Common;

namespace Tomeshelf.SHiFT.Infrastructure.Services;

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
