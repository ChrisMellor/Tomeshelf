using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.SHiFT.Application.Features.KeyDiscovery.Models;

namespace Tomeshelf.SHiFT.Application.Abstractions.External;

public interface IShiftKeySource
{
    string Name { get; }

    Task<IReadOnlyList<ShiftKeyCandidate>> GetKeysAsync(DateTimeOffset sinceUtc, CancellationToken cancellationToken = default);
}