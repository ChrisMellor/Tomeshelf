using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.SHiFT.Application.Features.KeyDiscovery.Models;

namespace Tomeshelf.SHiFT.Application.Abstractions.External;

public interface IShiftKeySource
{
    string Name { get; }

    /// <summary>
    ///     Gets the keys asynchronously.
    /// </summary>
    /// <param name="sinceUtc">The since utc.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the operation result.</returns>
    Task<IReadOnlyList<ShiftKeyCandidate>> GetKeysAsync(DateTimeOffset sinceUtc, CancellationToken cancellationToken = default);
}