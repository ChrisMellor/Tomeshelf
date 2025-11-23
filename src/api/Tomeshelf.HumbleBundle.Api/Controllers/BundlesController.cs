using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Tomeshelf.HumbleBundle.Api.Records;
using Tomeshelf.Infrastructure.Bundles;

namespace Tomeshelf.HumbleBundle.Api.Controllers;

[ApiController]
[Route("bundles")]
public sealed class BundlesController : ControllerBase
{
    private readonly BundleIngestService _ingest;
    private readonly ILogger<BundlesController> _logger;
    private readonly BundleQueries _queries;
    private readonly IHumbleBundleScraper _scraper;

    public BundlesController(BundleQueries queries, IHumbleBundleScraper scraper, BundleIngestService ingest, ILogger<BundlesController> logger)
    {
        _queries = queries;
        _scraper = scraper;
        _ingest = ingest;
        _logger = logger;
    }

    /// <summary>
    ///     Returns bundles captured from HumbleBundle.com. Includes remaining time until expiry.
    /// </summary>
    /// <param name="includeExpired">Set to true to include expired bundles.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<BundleResponse>), 200)]
    public async Task<ActionResult<IReadOnlyList<BundleResponse>>> GetBundles([FromQuery] bool includeExpired = false, CancellationToken cancellationToken = default)
    {
        var dtos = await _queries.GetBundlesAsync(includeExpired, cancellationToken);
        var now = DateTimeOffset.UtcNow;

        var result = dtos.Select(dto => BundleResponse.FromDto(dto, now))
                         .ToList();

        return Ok(result);
    }

    /// <summary>
    ///     Scrapes the Humble Bundle listing page and persists the latest data.
    /// </summary>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(RefreshBundlesResponse), 200)]
    public async Task<ActionResult<RefreshBundlesResponse>> RefreshBundles(CancellationToken cancellationToken = default)
    {
        var scraped = await _scraper.ScrapeAsync(cancellationToken);
        var ingestResult = await _ingest.UpsertAsync(scraped, cancellationToken);

        _logger.LogInformation("Bundles refresh completed via API call - processed {Processed} bundles (created: {Created}, updated: {Updated}, unchanged: {Unchanged})",
                               ingestResult.Processed, ingestResult.Created, ingestResult.Updated, ingestResult.Unchanged);

        return Ok(new RefreshBundlesResponse(ingestResult.Created, ingestResult.Updated, ingestResult.Unchanged, ingestResult.Processed, ingestResult.ObservedAtUtc));
    }
}