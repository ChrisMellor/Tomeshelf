using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.HumbleBundle.Application.HumbleBundle;

namespace Tomeshelf.HumbleBundle.Application.Abstractions.Persistence;

public interface IBundleQueries
{
    /// <summary>
    ///     Gets the bundles asynchronously.
    /// </summary>
    /// <param name="includeExpired">The include expired.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the operation result.</returns>
    Task<IReadOnlyList<BundleDto>> GetBundlesAsync(bool includeExpired, CancellationToken cancellationToken = default);
}