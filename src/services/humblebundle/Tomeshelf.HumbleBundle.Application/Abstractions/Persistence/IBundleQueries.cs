using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.HumbleBundle.Application.HumbleBundle;

namespace Tomeshelf.HumbleBundle.Application.Abstractions.Persistence;

public interface IBundleQueries
{
    Task<IReadOnlyList<BundleDto>> GetBundlesAsync(bool includeExpired, CancellationToken cancellationToken = default);
}