using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.HumbleBundle.Application.Features.Bundles.Models;

namespace Tomeshelf.HumbleBundle.Application.Abstractions.Persistence;

public interface IBundleIngestService
{
    /// <summary>
    ///     Inserts or updates asynchronously.
    /// </summary>
    /// <param name="bundles">The bundles.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the operation result.</returns>
    Task<BundleIngestResult> UpsertAsync(IReadOnlyList<ScrapedBundle> bundles, CancellationToken cancellationToken = default);
}