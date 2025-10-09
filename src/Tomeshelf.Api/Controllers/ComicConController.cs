using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Api.Enums;
using Tomeshelf.Api.Services;
using Tomeshelf.Infrastructure.Queries;
using Tomeshelf.Infrastructure.Services;

namespace Tomeshelf.Api.Controllers;

/// <summary>
/// Exposes endpoints for refreshing Comic Con guests and serving cached results.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class ComicConController : ControllerBase
{
    private readonly ILogger<ComicConController> _logger;
    private readonly IGuestService _guestService;
    private readonly GuestQueries _queries;
    private readonly IGuestsCache _cache;

    /// <summary>
    /// Initializes a new instance of the <see cref="ComicConController"/> class.
    /// </summary>
    /// <param name="guestService">Domain service for updating guests.</param>
    /// <param name="queries">Read-only guest queries.</param>
    /// <param name="logger">Logger instance.</param>
    /// <param name="cache">In-memory guests cache.</param>
    public ComicConController(IGuestService guestService, GuestQueries queries, ILogger<ComicConController> logger, IGuestsCache cache)
    {
        _guestService = guestService;
        _queries = queries;
        _logger = logger;
        _cache = cache;
    }

    /// <summary>
    /// Refreshes guests for the specified city, persists them, updates the cache, and returns the latest people list.
    /// </summary>
    /// <param name="city">The city to refresh (enum value).</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>An <see cref="IActionResult"/> with the updated guests or an error.</returns>
    [HttpPost("Guests/City", Name = "UpdateLatestGuests")]
    public async Task<IActionResult> UpdateGuests([FromQuery] City city, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating guests for {CityName}", city);

        try
        {
            switch (city)
            {
                case City.London:
                case City.Birmingham:
                    var cityName = city.ToString();
                    var newGuests = await _guestService.GetGuestsAsync(cityName, cancellationToken);

                    var groups = await _queries.GetGuestsByCityAsync(cityName, cancellationToken);
                    var total = groups.Sum(g => g.Items.Count);
                    _cache.Set(cityName, new GuestsSnapshot(cityName, total, groups, DateTimeOffset.UtcNow));

                    return Ok(newGuests);
                default:
                    throw new ArgumentOutOfRangeException(nameof(city), city, "This city is not available");
            }

        }
        catch (ApplicationException ex)
        {
            _logger.LogWarning(ex, "Failed to get guests for {CityName}", city);

            return NotFound(new { ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while updating guests for {CityName}", city);

            return StatusCode(500, new { Message = "An unexpected error occurred." });
        }
    }

    /// <summary>
    /// Returns grouped guests and totals for a city, serving from cache when available and refreshing snapshots on demand.
    /// </summary>
    /// <param name="city">City name to query (e.g., "london").</param>
    /// <param name="cancellationToken">Cancellation token for the query.</param>
    /// <returns>An <see cref="ActionResult"/> containing city, total and groups.</returns>
    [HttpGet("Guests/City")]
    public async Task<ActionResult<object>> GetByCity([FromQuery] string city, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(city))
        {
            return BadRequest(new { message = "Missing required 'city' query parameter." });
        }

        using var scope = _logger.BeginScope(new { City = city });
        _logger.LogInformation("Fetching guests by city (using cache if available)");
        var started = DateTimeOffset.UtcNow;

        if (_cache.TryGet(city, out var snapshot))
        {
            var durationHit = DateTimeOffset.UtcNow - started;
            _logger.LogInformation("Cache hit for {City} -> {Total} guests in {Duration}ms", city, snapshot.Total, (int)durationHit.TotalMilliseconds);
            return Ok(new { city = snapshot.City, total = snapshot.Total, groups = snapshot.Groups });
        }

        try
        {
            var groups = await _queries.GetGuestsByCityAsync(city, cancellationToken);
            var total = groups.Sum(g => g.Items.Count);
            var generatedUtc = DateTimeOffset.UtcNow;
            _cache.Set(city, new GuestsSnapshot(city, total, groups, generatedUtc));
            var durationMiss = DateTimeOffset.UtcNow - started;
            _logger.LogInformation("Cache miss for {City}; refreshed snapshot with {Total} guests in {Duration}ms", city, total, (int)durationMiss.TotalMilliseconds);

            return Ok(new { city, total, groups });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch guests for {City}", city);
            return StatusCode(500, new { message = "Failed to load guests." });
        }
    }
}
