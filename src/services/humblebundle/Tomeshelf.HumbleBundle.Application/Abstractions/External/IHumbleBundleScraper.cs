using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.HumbleBundle.Application.Features.Bundles.Models;

namespace Tomeshelf.HumbleBundle.Application.Abstractions.External;

public interface IHumbleBundleScraper
{
    Task<IReadOnlyList<ScrapedBundle>> ScrapeAsync(CancellationToken cancellationToken = default);
}