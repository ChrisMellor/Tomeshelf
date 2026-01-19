using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Application.SHiFT;
using Tomeshelf.SHiFT.Api.Contracts;

namespace Tomeshelf.SHiFT.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class GearboxController : ControllerBase
{
    private readonly IGearboxService _gearboxService;

    public GearboxController(IGearboxService gearboxService)
    {
        _gearboxService = gearboxService;
    }

    [HttpPost("redeem")]
    public async Task<IActionResult> Redeem([FromBody] RedeemRequestDto requestDto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(requestDto.Code))
        {
            return BadRequest("Code is required.");
        }

        await _gearboxService.RedeemCodeAsync(requestDto.Code, requestDto.Service, ct);

        return Ok();
    }
}