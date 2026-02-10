using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.HumbleBundle.Application.Features.Bundles.Models;

namespace Tomeshelf.HumbleBundle.Application.Abstractions.External;

public interface IHumbleBundleScraper
{
    /// <summary>
    ///     Scrapes asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the operation result.</returns>
    Task<IReadOnlyList<ScrapedBundle>> ScrapeAsync(CancellationToken cancellationToken = default);
}