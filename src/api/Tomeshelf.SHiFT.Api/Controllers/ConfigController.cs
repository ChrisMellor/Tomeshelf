using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Application.SHiFT;

namespace Tomeshelf.SHiFT.Api.Controllers;

[ApiController]
[Route("config/shift")]
public sealed class ConfigController : ControllerBase
{
    private readonly IShiftSettingsStore _store;

    public ConfigController(IShiftSettingsStore store)
    {
        _store = store;
    }

    [HttpGet]
    public async Task<ActionResult<ShiftSettingsDto>> Get(CancellationToken ct)
    {
        return Ok(await _store.GetAsync(ct));
    }

    [HttpPut]
    public async Task<IActionResult> Put([FromBody] ShiftSettingsUpdateRequest request, CancellationToken ct)
    {
        await _store.UpsertAsync(request, ct);

        return NoContent();
    }
}