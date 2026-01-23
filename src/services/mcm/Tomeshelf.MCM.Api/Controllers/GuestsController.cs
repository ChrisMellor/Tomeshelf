using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.MCM.Api.Contracts;
using Tomeshelf.MCM.Api.Models;
using Tomeshelf.MCM.Api.Services;

namespace Tomeshelf.MCM.Api.Controllers;

/// <summary>
///     Provides API endpoints for managing and synchronizing guests associated with a specific event.
/// </summary>
/// <remarks>
///     This controller exposes operations to retrieve paged guest lists and to synchronize guest data for a
///     given event. All routes are scoped to a particular event, identified by the event ID in the route. The controller
///     is
///     intended to be used in the context of event management scenarios where guest information must be queried or kept in
///     sync with external systems.
/// </remarks>
[ApiController]
[Route("events/{eventId}/guests")]
public class GuestsController : ControllerBase
{
    private readonly IGuestsService _guestsService;

    /// <summary>
    ///     Initializes a new instance of the GuestsController class with the specified guests service.
    /// </summary>
    /// <param name="guestsService">The service used to manage guest-related operations. Cannot be null.</param>
    public GuestsController(IGuestsService guestsService)
    {
        _guestsService = guestsService;
    }

    /// <summary>
    ///     Retrieves a paged list of guests for the specified event.
    /// </summary>
    /// <param name="eventId">The unique identifier of the event for which to retrieve guests.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <param name="page">The page number of results to retrieve. Must be greater than or equal to 1. The default is 1.</param>
    /// <param name="pageSize">
    ///     The maximum number of guests to include in a single page of results. Must be between 1 and 400.
    ///     The default is 50.
    /// </param>
    /// <param name="eventName">An optional event name to filter the results. If null, no filtering by event name is applied.</param>
    /// <param name="includeDeleted">
    ///     true to include guests that have been marked as deleted; otherwise, false. The default is
    ///     false.
    /// </param>
    /// <returns>
    ///     An <see cref="IActionResult" /> containing a <see cref="PagedResult{GuestDto}" /> with the list of guests for the
    ///     specified event. Returns a 200 OK response with the results, or a 400 Bad Request response if the input parameters
    ///     are invalid.
    /// </returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<GuestDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Get([FromRoute] string eventId, CancellationToken cancellationToken, [FromQuery] int page = 1, [FromQuery] int pageSize = 50, [FromQuery] string? eventName = null, [FromQuery] bool includeDeleted = false)
    {
        if (page < 1)
        {
            return ValidationProblem("page must be >= 1");
        }

        if (pageSize is < 1 or > 400)
        {
            return ValidationProblem("pageSize must be between 1 and 400");
        }

        var model = new EventConfigModel
        {
            Id = eventId,
            Name = eventName ?? string.Empty
        };

        var result = await _guestsService.GetAsync(model, page, pageSize, includeDeleted, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    ///     Synchronizes guest data for the specified event and returns the synchronization result.
    /// </summary>
    /// <param name="eventId">The unique identifier of the event for which to synchronize guest data. Cannot be null.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    ///     An <see cref="ActionResult{T}" /> containing a <see cref="GuestSyncResultDto" /> with the
    ///     synchronization result if
    ///     the event is found; otherwise, a 404 Not Found response.
    /// </returns>
    [HttpPost("sync")]
    [ProducesResponseType(typeof(GuestSyncResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GuestSyncResultDto>> Sync([FromRoute] string eventId, CancellationToken cancellationToken)
    {
        var model = new EventConfigModel
        {
            Id = eventId,
            Name = string.Empty
        };

        var result = await _guestsService.SyncAsync(model, cancellationToken);
        if (result is null)
        {
            return NotFound(eventId);
        }

        return Ok(result);
    }
}