using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.MCM.Api.Contracts;
using Tomeshelf.MCM.Api.Enums;
using Tomeshelf.MCM.Api.Services;

namespace Tomeshelf.MCM.Api.Controllers;

/// <summary>
///     Provides API endpoints for managing guest-related operations.
/// </summary>
/// <remarks>
///     This controller is intended to be used as part of an ASP.NET Core application. All routes are
///     prefixed with 'Guests', and actions within this controller handle requests related to guests. The controller is
///     decorated with the <see cref="ApiControllerAttribute" /> to enable automatic model validation and other
///     API-specific behaviors.
/// </remarks>
[ApiController]
[Route("[controller]")]
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
    ///     Synchronizes guest data for the specified city and returns the result of the synchronization operation.
    /// </summary>
    /// <param name="city">
    ///     The city for which guest data should be synchronized. Must be a valid city identifier provided in
    ///     the route.
    /// </param>
    /// <param name="cancellationToken">A token that can be used to cancel the synchronization operation.</param>
    /// <returns>
    ///     An <see cref="IActionResult" /> containing a <see cref="GuestSyncResultDto" /> with the results of the
    ///     synchronization. Returns status code 200 (OK) if successful.
    /// </returns>
    [HttpPost("{city}/sync")]
    [ProducesResponseType(typeof(GuestSyncResultDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Sync([FromRoute] City city, CancellationToken cancellationToken)
    {
        var syncResult = await _guestsService.SyncAsync(city, cancellationToken);

        return Ok(syncResult);
    }

    /// <summary>
    ///     Retrieves a paged list of guests for the specified city.
    /// </summary>
    /// <param name="city">The city for which to retrieve guest information.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <param name="page">The page number of results to retrieve. Must be greater than or equal to 1.</param>
    /// <param name="pageSize">The number of guests to include per page. Must be between 1 and 200.</param>
    /// <returns>
    ///     An <see cref="IActionResult" /> containing a paged result of guests for the specified city. Returns a validation
    ///     problem response if the page or pageSize parameters are outside their valid ranges.
    /// </returns>
    [HttpGet("{city}")]
    [ProducesResponseType(typeof(PagedResult<GuestDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get([FromRoute] City city, CancellationToken cancellationToken, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        if (page < 1)
        {
            return ValidationProblem("page must be >= 1");
        }

        if (pageSize is < 1 or > 200)
        {
            return ValidationProblem("pageSize must be between 1 and 200");
        }

        var result = await _guestsService.GetAsync(city, page, pageSize, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    ///     Deletes all guests associated with the specified city.
    /// </summary>
    /// <param name="city">An object representing the city for which guests should be deleted. Must not be null.</param>
    /// <returns>
    ///     An IActionResult indicating the result of the delete operation. Returns a success response if the guests were
    ///     deleted.
    /// </returns>
    [HttpDelete("{city}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteGuests([FromRoute] City city, CancellationToken cancellationToken)
    {
        await _guestsService.DeleteAsync(city, cancellationToken);

        return NoContent();
    }
}