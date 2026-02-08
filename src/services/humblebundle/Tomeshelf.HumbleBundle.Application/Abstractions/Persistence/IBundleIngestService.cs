using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.HumbleBundle.Application.Features.Bundles.Models;

namespace Tomeshelf.HumbleBundle.Application.Abstractions.Persistence;

public interface IBundleIngestService
{
    Task<BundleIngestResult> UpsertAsync(IReadOnlyList<ScrapedBundle> bundles, CancellationToken cancellationToken = default);
}