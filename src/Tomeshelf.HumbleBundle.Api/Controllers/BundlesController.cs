using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Application.Contracts;
using Tomeshelf.Infrastructure.Bundles;

namespace Tomeshelf.HumbleBundle.Api.Controllers;

[ApiController]
[Route("bundles")]
public sealed class BundlesController : ControllerBase
{
    private readonly BundleQueries _queries;
    private readonly IHumbleBundleScraper _scraper;
    private readonly BundleIngestService _ingest;
    private readonly ILogger<BundlesController> _logger;

    public BundlesController(BundleQueries queries, IHumbleBundleScraper scraper, BundleIngestService ingest, ILogger<BundlesController> logger)
    {
        _queries = queries;
        _scraper = scraper;
        _ingest = ingest;
        _logger = logger;
    }

    /// <summary>
    /// Returns bundles captured from HumbleBundle.com. Includes remaining time until expiry.
    /// </summary>
    /// <param name="includeExpired">Set to true to include expired bundles.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<BundleResponse>), 200)]
    public async Task<ActionResult<IReadOnlyList<BundleResponse>>> GetBundles([FromQuery] bool includeExpired = false, CancellationToken cancellationToken = default)
    {
        var dtos = await _queries.GetBundlesAsync(includeExpired, cancellationToken);
        var now = DateTimeOffset.UtcNow;

        var result = dtos
            .Select(dto => BundleResponse.FromDto(dto, now))
            .ToList();

        return Ok(result);
    }

    /// <summary>
    /// Scrapes the Humble Bundle listing page and persists the latest data.
    /// </summary>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(RefreshBundlesResponse), 200)]
    public async Task<ActionResult<RefreshBundlesResponse>> RefreshBundles(CancellationToken cancellationToken = default)
    {
        var scraped = await _scraper.ScrapeAsync(cancellationToken);
        var ingestResult = await _ingest.UpsertAsync(scraped, cancellationToken);

        _logger.LogInformation(
            "Bundles refresh completed via API call - processed {Processed} bundles (created: {Created}, updated: {Updated}, unchanged: {Unchanged})",
            ingestResult.Processed,
            ingestResult.Created,
            ingestResult.Updated,
            ingestResult.Unchanged);

        return Ok(new RefreshBundlesResponse(
            ingestResult.Created,
            ingestResult.Updated,
            ingestResult.Unchanged,
            ingestResult.Processed,
            ingestResult.ObservedAtUtc));
    }

    /// <summary>
    /// API model returned by the GET endpoint, enriched with computed remaining time.
    /// </summary>
    /// <param name="MachineName">Stable identifier.</param>
    /// <param name="Category">Bundle category.</param>
    /// <param name="Stamp">Bundle type stamp.</param>
    /// <param name="Title">Full display title.</param>
    /// <param name="ShortName">Short marketing title.</param>
    /// <param name="Url">Product URL.</param>
    /// <param name="TileImageUrl">Tile image.</param>
    /// <param name="TileLogoUrl">Tile logo.</param>
    /// <param name="HeroImageUrl">Hero image.</param>
    /// <param name="ShortDescription">Short description text.</param>
    /// <param name="StartsAt">Start date.</param>
    /// <param name="EndsAt">End date.</param>
    /// <param name="FirstSeenUtc">First time the scraper observed the bundle.</param>
    /// <param name="LastSeenUtc">Last time the bundle was observed.</param>
    /// <param name="LastUpdatedUtc">Last time metadata changed.</param>
    /// <param name="SecondsRemaining">Seconds remaining until expiry, when applicable.</param>
    /// <param name="GeneratedUtc">Timestamp when this projection was generated.</param>
    public sealed record BundleResponse(
        string MachineName,
        string Category,
        string Stamp,
        string Title,
        string ShortName,
        string Url,
        string TileImageUrl,
        string TileLogoUrl,
        string HeroImageUrl,
        string ShortDescription,
        DateTimeOffset? StartsAt,
        DateTimeOffset? EndsAt,
        DateTimeOffset FirstSeenUtc,
        DateTimeOffset LastSeenUtc,
        DateTimeOffset LastUpdatedUtc,
        double? SecondsRemaining,
        DateTimeOffset GeneratedUtc)
    {
        public static BundleResponse FromDto(BundleDto dto, DateTimeOffset now)
        {
            double? secondsRemaining = null;
            if (dto.EndsAt.HasValue)
            {
                var remaining = dto.EndsAt.Value - now;
                if (remaining > TimeSpan.Zero)
                {
                    secondsRemaining = remaining.TotalSeconds;
                }
            }

            return new BundleResponse(
                dto.MachineName,
                dto.Category,
                dto.Stamp,
                dto.Title,
                dto.ShortName,
                dto.Url,
                dto.TileImageUrl,
                dto.TileLogoUrl,
                dto.HeroImageUrl,
                dto.ShortDescription,
                dto.StartsAt,
                dto.EndsAt,
                dto.FirstSeenUtc,
                dto.LastSeenUtc,
                dto.LastUpdatedUtc,
                secondsRemaining,
                dto.GeneratedUtc);
        }
    }

    /// <summary>
    /// Summary returned after invoking the refresh endpoint.
    /// </summary>
    /// <param name="Created">Bundles created during the ingest.</param>
    /// <param name="Updated">Bundles updated during the ingest.</param>
    /// <param name="Unchanged">Bundles left unchanged.</param>
    /// <param name="Processed">Total bundles processed.</param>
    /// <param name="ObservedAtUtc">Observation timestamp supplied by the ingest.</param>
    public sealed record RefreshBundlesResponse(
        int Created,
        int Updated,
        int Unchanged,
        int Processed,
        DateTimeOffset ObservedAtUtc);
}
