using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using Tomeshelf.MCM.Api.Enums;

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
    private readonly ILogger<GuestsController> _logger;

    /// <summary>
    ///     Initializes a new instance of the GuestsController class with the specified logger.
    /// </summary>
    /// <param name="logger">
    ///     The logger instance used to record diagnostic and operational information for the controller.
    ///     Cannot be null.
    /// </param>
    public GuestsController(ILogger<GuestsController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    ///     Synchronizes guest information for the specified city.
    /// </summary>
    /// <remarks>
    ///     This action is typically used to trigger a manual update of guest data for a given city. The
    ///     operation is performed via an HTTP POST request to the endpoint formatted as '/{city}/sync'.
    /// </remarks>
    /// <param name="city">The city for which guest data will be updated. Cannot be null.</param>
    /// <returns>
    ///     An <see cref="IActionResult" /> indicating the result of the synchronization operation. Returns a success message
    ///     if the update completes successfully.
    /// </returns>
    [HttpPost("{city}/sync")]
    public IActionResult Sync([FromRoute] City city)
    {
        _logger.LogInformation("UpdateGuests called for city: {City}", city);
        // Placeholder implementation
        _logger.LogInformation("Guests updated successfully for city: {City}", city);

        return Ok(new
        {
            city,
            status = "Succeeded"
        });
    }

    /// <summary>
    ///     Retrieves a paginated list of guests for the specified city.
    /// </summary>
    /// <param name="city">
    ///     The city for which to retrieve guest information. This value is provided from the route and must correspond to a
    ///     valid city.
    /// </param>
    /// <param name="page">The page number of results to return. Must be greater than or equal to 1. Defaults to 1.</param>
    /// <param name="pageSize">
    ///     The maximum number of guests to include in a single page of results. Must be greater than 0.
    ///     Defaults to 50.
    /// </param>
    /// <returns>An <see cref="IActionResult" /> containing the guest data for the specified city and page.</returns>
    [HttpGet("{city}")]
    public IActionResult Get([FromRoute] City city, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        _logger.LogInformation("Get called for city: {City}", city);
        // Placeholder implementation
        _logger.LogInformation("Guests retrieved successfully for city: {City}", city);

        return Ok(new
        {
            city,
            page,
            pageSize,
            guests = Array.Empty<object>()
        });
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
    public IActionResult DeleteGuests([FromRoute] City city)
    {
        _logger.LogInformation("DeleteGuests called for city: {City}", city);
        // Placeholder implementation
        _logger.LogInformation("Guests deleted successfully for city: {City}", city);

        return NoContent();
    }
}