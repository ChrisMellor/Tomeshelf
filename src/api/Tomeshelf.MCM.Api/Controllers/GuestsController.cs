using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Mcm.Api.Contracts;
using Tomeshelf.Mcm.Api.Models;
using Tomeshelf.Mcm.Api.Services;

namespace Tomeshelf.Mcm.Api.Controllers;

/// <summary>
///     Handles HTTP requests related to guest management for a specific event configuration.
/// </summary>
/// <remarks>
///     This controller provides endpoints to synchronize, retrieve, and delete guests associated with an
///     event. All actions require an event configuration to be specified in the route. The controller relies on an
///     injected
///     guests service to perform guest-related operations. Responses follow standard HTTP status codes for success and
///     validation errors.
/// </remarks>
[ApiController]
[Route("events/{eventId:guid}/guests")]
public class GuestsController : ControllerBase
{
    private readonly IGuestsService _guestsService;
    private readonly IGuestSyncService _guestSyncService;

    /// <summary>
    ///     Initializes a new instance of the GuestsController class with the specified services.
    /// </summary>
    /// <param name="guestsService">The service used to manage guest data and operations. Cannot be null.</param>
    /// <param name="guestSyncService">
    ///     The service responsible for synchronizing guest information with external systems.
    ///     Cannot be null.
    /// </param>
    public GuestsController(IGuestsService guestsService, IGuestSyncService guestSyncService)
    {
        _guestsService = guestsService;
        _guestSyncService = guestSyncService;
    }

    /// <summary>
    ///     Deletes all guests associated with the specified event.
    /// </summary>
    /// <remarks>
    ///     This action removes all guests linked to the event identified by <paramref name="eventId" />.
    ///     If no guests exist for the event, the operation completes without error. The response is always HTTP 204 (No
    ///     Content).
    /// </remarks>
    /// <param name="eventId">The unique identifier of the event for which guests will be deleted.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the delete operation.</param>
    /// <returns>A result indicating that the operation completed successfully with no content.</returns>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteGuests([FromRoute] Guid eventId, CancellationToken cancellationToken)
    {
        var model = new EventConfigModel
        {
            Id = eventId,
            Name = string.Empty
        };
        await _guestsService.DeleteAsync(model, cancellationToken);

        return NoContent();
    }

    /// <summary>
    ///     Retrieves a paged list of guests for the specified event.
    /// </summary>
    /// <param name="eventId">The unique identifier of the event for which guests are to be retrieved.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <param name="page">The page number of results to retrieve. Must be greater than or equal to 1.</param>
    /// <param name="pageSize">The number of guests to include per page. Must be between 1 and 200.</param>
    /// <param name="eventName">
    ///     An optional event name to filter the guests by. If not specified, all guests for the event are
    ///     returned.
    /// </param>
    /// <returns>
    ///     An <see cref="IActionResult" /> containing a <see cref="PagedResult{GuestDto}" /> with the guests for the
    ///     specified event. Returns a 400 Bad Request response if the page or pageSize parameters are out of range.
    /// </returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<GuestDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Get([FromRoute] Guid eventId, CancellationToken cancellationToken, [FromQuery] int page = 1, [FromQuery] int pageSize = 50, [FromQuery] string? eventName = null)
    {
        if (page < 1)
        {
            return ValidationProblem("page must be >= 1");
        }

        if (pageSize is < 1 or > 200)
        {
            return ValidationProblem("pageSize must be between 1 and 200");
        }

        var model = new EventConfigModel
        {
            Id = eventId,
            Name = eventName ?? string.Empty
        };
        var result = await _guestsService.GetAsync(model, page, pageSize, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    ///     Synchronizes guest data for the specified event.
    /// </summary>
    /// <param name="eventId">The unique identifier of the event for which guest data should be synchronized.</param>
    /// <param name="cancellationToken">A token that can be used to request cancellation of the operation.</param>
    /// <returns>
    ///     An <see cref="IActionResult" /> containing the result of the synchronization operation. Returns a 200 OK response
    ///     with a <see cref="GuestSyncResultDto" /> on success.
    /// </returns>
    [HttpPost("sync")]
    [ProducesResponseType(typeof(GuestSyncResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult<GuestSyncResultDto>> Sync([FromRoute] Guid eventId, CancellationToken cancellationToken)
    {
        var snapshotRecords = await _guestSyncService.SyncGuestsAsync(eventId, cancellationToken);

        return Ok();
    }
}