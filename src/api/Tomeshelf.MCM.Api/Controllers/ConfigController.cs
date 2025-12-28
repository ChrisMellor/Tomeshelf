using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Mcm.Api.Models;
using Tomeshelf.Mcm.Api.Services;

namespace Tomeshelf.Mcm.Api.Controllers;

/// <summary>
///     Defines API endpoints for managing application configuration settings.
/// </summary>
/// <remarks>
///     This controller provides actions to update configuration values via HTTP requests. It is intended to
///     be used by authorized clients to modify application settings at runtime. All routes are relative to the controller
///     name (e.g., '/Config').
/// </remarks>
[ApiController]
[Route("[controller]")]
public class ConfigController : ControllerBase
{
    private readonly IEventService _eventService;

    /// <summary>
    ///     Initializes a new instance of the ConfigController class with the specified event configuration service.
    /// </summary>
    /// <param name="eventService">The service used to manage and retrieve event configuration data. Cannot be null.</param>
    public ConfigController(IEventService eventService)
    {
        _eventService = eventService;
    }

    /// <summary>
    ///     Deletes the event configuration with the specified identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the event configuration to delete.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the delete operation.</param>
    /// <returns>
    ///     A 204 No Content response if the event configuration was deleted successfully; otherwise, a 404 Not Found response
    ///     if no configuration with the specified identifier exists.
    /// </returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var isDeleted = await _eventService.DeleteAsync(id, cancellationToken);

        if (!isDeleted)
        {
            return NotFound(id);
        }

        return NoContent();
    }

    /// <summary>
    ///     Retrieves all event configuration models.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>
    ///     An <see cref="IActionResult" /> containing a collection of <see cref="EventConfigModel" /> objects with HTTP status
    ///     code 200 (OK).
    /// </returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<EventConfigModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var eventConfigs = await _eventService.GetAllAsync(cancellationToken);

        return Ok(eventConfigs);
    }

    /// <summary>
    ///     Updates the event configuration with the specified settings.
    /// </summary>
    /// <param name="model">The event configuration data to update. Cannot be null.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    ///     An <see cref="IActionResult" /> that indicates the result of the update operation. Returns HTTP 202 (Accepted) if
    ///     the
    ///     update is successful.
    /// </returns>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> Update(EventConfigModel model, CancellationToken cancellationToken)
    {
        await _eventService.UpsertAsync(model, cancellationToken);

        return Ok();
    }
}