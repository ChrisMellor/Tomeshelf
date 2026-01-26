using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.MCM.Application.Abstractions.Messaging;
using Tomeshelf.MCM.Application.Features.Events.Commands;
using Tomeshelf.MCM.Application.Features.Events.Queries;
using Tomeshelf.MCM.Application.Models;

namespace Tomeshelf.MCM.Api.Controllers;

/// <summary>
///     Defines API endpoints for managing event configuration data, including retrieving, updating, and deleting event
///     configurations.
/// </summary>
/// <remarks>
///     This controller provides RESTful endpoints for event configuration management. All actions require
///     appropriate authorization as configured in the application. The controller is registered at the route 'Config', and
///     all endpoints are relative to this base route. Thread safety and request handling are managed by ASP.NET Core's
///     controller infrastructure.
/// </remarks>
[ApiController]
[Route("[controller]")]
public class ConfigController : ControllerBase
{
    private readonly ICommandHandler<DeleteEventCommand, bool> _deleteHandler;
    private readonly IQueryHandler<GetEventsQuery, IReadOnlyList<EventConfigModel>> _getHandler;
    private readonly ICommandHandler<UpsertEventCommand, bool> _upsertHandler;

    /// <summary>
    ///     Initializes a new instance of the ConfigController class with the specified handlers.
    /// </summary>
    /// <param name="getHandler">Query handler for retrieving event configurations.</param>
    /// <param name="upsertHandler">Command handler for inserting or updating event configurations.</param>
    /// <param name="deleteHandler">Command handler for deleting event configurations.</param>
    public ConfigController(
        IQueryHandler<GetEventsQuery, IReadOnlyList<EventConfigModel>> getHandler,
        ICommandHandler<UpsertEventCommand, bool> upsertHandler,
        ICommandHandler<DeleteEventCommand, bool> deleteHandler)
    {
        _getHandler = getHandler;
        _upsertHandler = upsertHandler;
        _deleteHandler = deleteHandler;
    }

    /// <summary>
    ///     Deletes the event with the specified identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the event to delete. Cannot be null or empty.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the delete operation.</param>
    /// <returns>
    ///     A 204 No Content response if the event was successfully deleted; otherwise, a 404 Not Found response if no event
    ///     with the specified identifier exists.
    /// </returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete([FromRoute] string id, CancellationToken cancellationToken)
    {
        var isDeleted = await _deleteHandler.Handle(new DeleteEventCommand(id), cancellationToken);

        if (!isDeleted)
        {
            return NotFound(id);
        }

        return NoContent();
    }

    /// <summary>
    ///     Retrieves a collection of event configuration models.
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
        var eventConfigs = await _getHandler.Handle(new GetEventsQuery(), cancellationToken);

        return Ok(eventConfigs);
    }

    /// <summary>
    ///     Updates the event configuration with the specified values.
    /// </summary>
    /// <param name="model">The event configuration data to update. Cannot be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the update operation.</param>
    /// <returns>
    ///     An <see cref="IActionResult" /> indicating the result of the update operation. Returns 200 OK if the update is
    ///     successful.
    /// </returns>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> Update(EventConfigModel model, CancellationToken cancellationToken)
    {
        await _upsertHandler.Handle(new UpsertEventCommand(model), cancellationToken);

        return Ok();
    }
}