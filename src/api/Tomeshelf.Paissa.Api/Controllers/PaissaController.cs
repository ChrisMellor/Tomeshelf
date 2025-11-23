using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tomeshelf.Paissa.Api.Records;
using Tomeshelf.Paissa.Api.Services;

namespace Tomeshelf.Paissa.Api.Controllers;

[ApiController]
[Route("paissa")]
public sealed class PaissaController : ControllerBase
{
    private readonly PaissaHousingService _housingService;

    public PaissaController(PaissaHousingService housingService)
    {
        _housingService = housingService;
    }

    /// <summary>
    ///     Returns the housing plots currently accepting entries for the configured world.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    [HttpGet("world")]
    [ProducesResponseType(typeof(PaissaWorldResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaissaWorldResponse>> GetWorld(CancellationToken cancellationToken)
    {
        var world = await _housingService.GetAcceptingEntriesAsync(cancellationToken);

        return Ok(world);
    }
}