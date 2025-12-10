using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
[Route("[controller]/{model}")]
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
    ///     Synchronizes guest data for the specified event configuration.
    /// </summary>
    /// <param name="model">
    ///     The event configuration model containing details of the event for which guest data should be synchronized. Must
    ///     not be null.
    /// </param>
    /// <param name="cancellationToken">
    ///     A token to monitor for cancellation requests. Passing a non-default token allows the
    ///     operation to be cancelled.
    /// </param>
    /// <returns>
    ///     An <see cref="IActionResult" /> containing a <see cref="GuestSyncResultDto" /> with the results of the
    ///     synchronization operation. Returns HTTP 200 (OK) with the result data.
    /// </returns>
    [HttpPost("sync")]
    [ProducesResponseType(typeof(GuestSyncResultDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Sync([FromBody] EventConfigModel model, CancellationToken cancellationToken)
    {
        var syncResult = await _guestsService.SyncAsync(model, cancellationToken);

        return Ok(syncResult);
    }

    /// <summary>
    ///     Retrieves a paged list of guests for the specified event configuration.
    /// </summary>
    /// <param name="model">
    ///     The event configuration identifying the event for which to retrieve guests. Must be provided in the
    ///     route.
    /// </param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="page">The page number to retrieve. Must be greater than or equal to 1. Defaults to 1.</param>
    /// <param name="pageSize">The number of guests to include per page. Must be between 1 and 200. Defaults to 50.</param>
    /// <returns>
    ///     An <see cref="IActionResult" /> containing a <see cref="PagedResult{GuestDto}" /> with the guests for the
    ///     specified event and page. Returns a 400 Bad Request response if the page or pageSize parameters are out of
    ///     range.
    /// </returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<GuestDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Get([FromRoute] EventConfigModel model, CancellationToken cancellationToken, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        if (page < 1)
        {
            return ValidationProblem("page must be >= 1");
        }

        if (pageSize is < 1 or > 200)
        {
            return ValidationProblem("pageSize must be between 1 and 200");
        }

        var result = await _guestsService.GetAsync(model, page, pageSize, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    ///     Deletes all guests associated with the specified event configuration.
    /// </summary>
    /// <param name="model">The event configuration identifying the guests to delete. Must not be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the delete operation.</param>
    /// <returns>A response indicating that the guests were successfully deleted. Returns HTTP 204 No Content on success.</returns>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteGuests([FromRoute] EventConfigModel model, CancellationToken cancellationToken)
    {
        await _guestsService.DeleteAsync(model, cancellationToken);

        return NoContent();
    }
}