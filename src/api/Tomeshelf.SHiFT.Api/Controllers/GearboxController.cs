using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Application.SHiFT;

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
    public async Task<IActionResult> Redeem([FromBody] RedeemRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Code))
        {
            return BadRequest("Code is required.");
        }

        await _gearboxService.RedeemCodeAsync(request.Code, request.Service, ct);

        return Ok();
    }

    public sealed record RedeemRequest(string Code, string? Service);
}