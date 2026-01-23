using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Application.Shared.Abstractions.SHiFT;
using Tomeshelf.SHiFT.Api.Contracts;

namespace Tomeshelf.SHiFT.Api.Controllers;

/// <summary>
///     Represents an API controller that manages gearbox-related operations.
/// </summary>
/// <remarks>
///     This controller provides endpoints for clients to interact with gearbox services, such as redeeming
///     codes. It is intended to be used as part of a web API and is configured with routing and API controller attributes
///     for integration with ASP.NET Core's routing and model binding features.
/// </remarks>
[ApiController]
[Route("[controller]")]
public class GearboxController : ControllerBase
{
    private readonly IGearboxService _gearboxService;

    /// <summary>
    ///     Initializes a new instance of the GearboxController class with the specified gearbox service.
    /// </summary>
    /// <param name="gearboxService">The service used to manage gearbox operations. Cannot be null.</param>
    public GearboxController(IGearboxService gearboxService)
    {
        _gearboxService = gearboxService;
    }

    /// <summary>
    ///     Processes a redemption request for a code and returns the result of the operation.
    /// </summary>
    /// <param name="requestDto">
    ///     The request data containing the code to redeem and the associated service information. Cannot be null. The code
    ///     property must not be null or whitespace.
    /// </param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>
    ///     An <see cref="IActionResult" /> indicating the result of the redemption operation. Returns a 200 OK response if
    ///     the code is successfully redeemed; otherwise, returns a 400 Bad Request if the code is missing or invalid.
    /// </returns>
    [HttpPost("redeem")]
    public async Task<IActionResult> Redeem([FromBody] RedeemRequestDto requestDto, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(requestDto.Code))
        {
            return BadRequest("Code is required.");
        }

        var results = await _gearboxService.RedeemCodeAsync(requestDto.Code, requestDto.Service, cancellationToken);

        var total = results.Count;
        var succeeded = results.Count(r => r.Success);
        var summary = new RedeemSummaryDto(total, succeeded, total - succeeded);

        return Ok(new RedeemResponseDto(summary, results));
    }
}