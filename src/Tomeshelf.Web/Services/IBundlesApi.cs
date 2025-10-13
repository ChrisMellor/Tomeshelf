using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Web.Models.Bundles;

namespace Tomeshelf.Web.Services;

/// <summary>
/// Abstraction for retrieving Humble Bundle listings from the backend API.
/// </summary>
public interface IBundlesApi
{
    /// <summary>
    /// Retrieves bundles from the Humble Bundle API.
    /// </summary>
    /// <param name="includeExpired">When true, includes bundles whose end date has passed.</param>
    /// <param name="cancellationToken">Cancellation token for the HTTP request.</param>
    Task<IReadOnlyList<BundleModel>> GetBundlesAsync(bool includeExpired, CancellationToken cancellationToken);
}
