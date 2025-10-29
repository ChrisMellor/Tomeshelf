using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tomeshelf.Paissa.Api.Models;
using Tomeshelf.Paissa.Api.Services;

namespace Tomeshelf.Paissa.Api.Controllers;

[ApiController]
[Route("paissa")]
public sealed class PaissaController(PaissaHousingService housingService) : ControllerBase
{
    /// <summary>
    ///     Returns the housing plots currently accepting entries for the configured world.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    [HttpGet("world")]
    [ProducesResponseType(typeof(PaissaWorldResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaissaWorldResponse>> GetWorld(CancellationToken cancellationToken)
    {
        var world = await housingService.GetAcceptingEntriesAsync(cancellationToken);

        return Ok(world);
    }
}