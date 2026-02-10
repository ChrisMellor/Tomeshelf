using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Tomeshelf.Application.Shared.Abstractions.Messaging;
using Tomeshelf.SHiFT.Api.Contracts;
using Tomeshelf.SHiFT.Application.Features.Settings.Commands;
using Tomeshelf.SHiFT.Application.Features.Settings.Dtos;
using Tomeshelf.SHiFT.Application.Features.Settings.Queries;

namespace Tomeshelf.SHiFT.Api.Controllers;

/// <summary>
///     API controller for retrieving and updating shift configuration settings.
/// </summary>
/// <remarks>
///     This controller provides endpoints to get the current shift settings and to update them. All actions
///     require appropriate permissions as determined by the application's security policies. The controller is intended to
///     be used by administrative or configuration management clients.
/// </remarks>
[ApiController]
[Route("config/shift")]
public sealed class ConfigController : ControllerBase
{
    private readonly ICommandHandler<CreateShiftSettingsCommand, int> _createHandler;
    private readonly ICommandHandler<DeleteShiftSettingsCommand, bool> _deleteHandler;
    private readonly IQueryHandler<ListShiftSettingsQuery, IReadOnlyList<ShiftSettingsDto>> _listHandler;
    private readonly IQueryHandler<GetShiftSettingsQuery, ShiftSettingsDto?> _queryHandler;
    private readonly ICommandHandler<UpdateShiftSettingsCommand, bool> _updateHandler;

    /// <summary>
    ///     Initializes a new instance of the ConfigController class using the specified handlers.
    /// </summary>
    /// <param name="queryHandler">Query handler for retrieving shift settings.</param>
    /// <param name="listHandler">Query handler for listing shift settings.</param>
    /// <param name="createHandler">Command handler for creating shift settings.</param>
    /// <param name="updateHandler">Command handler for updating shift settings.</param>
    /// <param name="deleteHandler">Command handler for deleting shift settings.</param>
    public ConfigController(IQueryHandler<GetShiftSettingsQuery, ShiftSettingsDto?> queryHandler, IQueryHandler<ListShiftSettingsQuery, IReadOnlyList<ShiftSettingsDto>> listHandler, ICommandHandler<CreateShiftSettingsCommand, int> createHandler, ICommandHandler<UpdateShiftSettingsCommand, bool> updateHandler, ICommandHandler<DeleteShiftSettingsCommand, bool> deleteHandler)
    {
        _queryHandler = queryHandler;
        _listHandler = listHandler;
        _createHandler = createHandler;
        _updateHandler = updateHandler;
        _deleteHandler = deleteHandler;
    }

    /// <summary>
    ///     Gets the all.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the operation result.</returns>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ShiftSettingsDto>>> GetAll(CancellationToken cancellationToken)
    {
        var dtos = await _listHandler.Handle(new ListShiftSettingsQuery(), cancellationToken);

        return Ok(dtos);
    }

    /// <summary>
    ///     Deletes.
    /// </summary>
    /// <param name="id">The id.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the operation result.</returns>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        _ = await _deleteHandler.Handle(new DeleteShiftSettingsCommand(id), cancellationToken);

        return NoContent();
    }

    /// <summary>
    ///     Retrieves the shift settings for the specified shift identifier.
    /// </summary>
    /// <remarks>
    ///     Returns an HTTP 200 response with the shift settings if the shift exists. If the shift does
    ///     not exist, the response will indicate a not found result. This method is asynchronous and supports cancellation
    ///     via the <paramref name="cancellationToken" /> parameter.
    /// </remarks>
    /// <param name="id">The unique identifier of the shift whose settings are to be retrieved.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>
    ///     An <see cref="ActionResult{ShiftSettingsDto}" /> containing the shift settings if found; otherwise, a result
    ///     indicating that the shift was not found.
    /// </returns>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ShiftSettingsDto>> Get(int id, CancellationToken cancellationToken)
    {
        var dto = await _queryHandler.Handle(new GetShiftSettingsQuery(id), cancellationToken);

        return dto is null
            ? NotFound()
            : Ok(dto);
    }

    /// <summary>
    ///     Posts.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the operation result.</returns>
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] ShiftSettingsUpdateRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var command = new CreateShiftSettingsCommand(request.Email, request.Password, request.DefaultService);
            var id = await _createHandler.Handle(command, cancellationToken);

            return CreatedAtAction(nameof(Get), new { id }, null);
        }
        catch (InvalidOperationException)
        {
            return Conflict("SHiFT email already exists.");
        }
    }

    /// <summary>
    ///     Updates the shift settings for the specified user.
    /// </summary>
    /// <param name="id">The unique identifier of the user whose shift settings are to be updated.</param>
    /// <param name="request">An object containing the updated shift settings to apply to the user. Cannot be null.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    ///     An <see cref="IActionResult" /> indicating the result of the update operation. Returns
    ///     <see
    ///         cref="NotFoundResult" />
    ///     if the user does not exist, <see cref="ConflictResult" /> if the update would result in a
    ///     duplicate SHiFT email, or <see cref="NoContentResult" /> if the update is successful.
    /// </returns>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Put(int id, [FromBody] ShiftSettingsUpdateRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var command = new UpdateShiftSettingsCommand(id, request.Email, request.Password, request.DefaultService);
            var updated = await _updateHandler.Handle(command, cancellationToken);

            return updated
                ? NoContent()
                : NotFound();
        }
        catch (InvalidOperationException)
        {
            return Conflict("SHiFT email already exists.");
        }
    }
}
