using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Application.Contracts.SHiFT;
using Tomeshelf.Application.SHiFT;

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
    private readonly IShiftSettingsStore _store;

    /// <summary>
    ///     Initializes a new instance of the ConfigController class using the specified shift settings store.
    /// </summary>
    /// <param name="store">The store that provides access to shift configuration settings. Cannot be null.</param>
    public ConfigController(IShiftSettingsStore store)
    {
        _store = store;
    }

    /// <summary>
    ///     Retrieves the current shift settings.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>
    ///     An <see cref="ActionResult{T}" /> containing the current shift settings as a <see cref="ShiftSettingsDto" />
    ///     object.
    /// </returns>
    [HttpGet]
    public async Task<ActionResult<ShiftSettingsDto>> Get(CancellationToken cancellationToken)
    {
        return Ok(await _store.GetAsync(cancellationToken));
    }

    /// <summary>
    ///     Updates the shift settings with the values provided in the request.
    /// </summary>
    /// <param name="request">The shift settings update data to apply. Cannot be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>
    ///     A result indicating the outcome of the update operation. Returns a 204 No Content response if the update is
    ///     successful.
    /// </returns>
    [HttpPut]
    public async Task<IActionResult> Put([FromBody] ShiftSettingsUpdateRequest request, CancellationToken cancellationToken)
    {
        await _store.UpsertAsync(request, cancellationToken);

        return NoContent();
    }
}