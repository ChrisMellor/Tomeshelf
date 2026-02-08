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

    public RefreshBundlesCommandHandler(IHumbleBundleScraper scraper, IBundleIngestService ingestService)
    {
        _scraper = scraper;
        _ingestService = ingestService;
    }

    public async Task<BundleIngestResult> Handle(RefreshBundlesCommand command, CancellationToken cancellationToken)
    {
        var scraped = await _scraper.ScrapeAsync(cancellationToken);

        return await _ingestService.UpsertAsync(scraped, cancellationToken);
    }
}