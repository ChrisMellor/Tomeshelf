using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Tomeshelf.Infrastructure.Bundles;

/// <summary>
///     Abstraction for retrieving Humble Bundle listing data from the public website.
/// </summary>
public interface IHumbleBundleScraper
{
    /// <summary>
    ///     Fetches the latest bundles from the Humble Bundle website.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the HTTP request.</param>
    /// <returns>A collection of scraped bundles.</returns>
    Task<IReadOnlyList<ScrapedBundle>> ScrapeAsync(CancellationToken cancellationToken = default);
}