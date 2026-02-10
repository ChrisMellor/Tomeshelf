using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Application.Shared.Abstractions.Messaging;
using Tomeshelf.HumbleBundle.Application.Abstractions.External;
using Tomeshelf.HumbleBundle.Application.Abstractions.Persistence;
using Tomeshelf.HumbleBundle.Application.Features.Bundles.Models;

namespace Tomeshelf.HumbleBundle.Application.Features.Bundles.Commands;

public sealed class RefreshBundlesCommandHandler : ICommandHandler<RefreshBundlesCommand, BundleIngestResult>
{
    private readonly IBundleIngestService _ingestService;
    private readonly IHumbleBundleScraper _scraper;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RefreshBundlesCommandHandler" /> class.
    /// </summary>
    /// <param name="scraper">The scraper.</param>
    /// <param name="ingestService">The ingest service.</param>
    public RefreshBundlesCommandHandler(IHumbleBundleScraper scraper, IBundleIngestService ingestService)
    {
        _scraper = scraper;
        _ingestService = ingestService;
    }

    /// <summary>
    ///     Handles.
    /// </summary>
    /// <param name="command">The command.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the operation result.</returns>
    public async Task<BundleIngestResult> Handle(RefreshBundlesCommand command, CancellationToken cancellationToken)
    {
        var scraped = await _scraper.ScrapeAsync(cancellationToken);

        return await _ingestService.UpsertAsync(scraped, cancellationToken);
    }
}