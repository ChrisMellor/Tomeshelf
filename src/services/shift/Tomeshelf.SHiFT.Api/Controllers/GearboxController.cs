using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Application.Shared.Abstractions.Messaging;
using Tomeshelf.SHiFT.Api.Contracts;
using Tomeshelf.SHiFT.Application;
using Tomeshelf.SHiFT.Application.Features.KeyDiscovery.Commands;
using Tomeshelf.SHiFT.Application.Features.KeyDiscovery.Models;
using Tomeshelf.SHiFT.Application.Features.Redemption.Commands;
using Tomeshelf.SHiFT.Application.Features.Redemption.Redeem;

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
    private readonly ICommandHandler<RedeemShiftCodeCommand, IReadOnlyList<RedeemResult>> _handler;
    private readonly IOptionsMonitor<ShiftKeyScannerOptions> _options;
    private readonly ICommandHandler<SweepShiftKeysCommand, ShiftKeySweepResult> _sweepHandler;

    /// <summary>
    ///     Initializes a new instance of the GearboxController class with the specified gearbox service.
    /// </summary>
    /// <param name="gearboxClient">The service used to manage gearbox operations. Cannot be null.</param>
    public GearboxController(ICommandHandler<RedeemShiftCodeCommand, IReadOnlyList<RedeemResult>> handler, ICommandHandler<SweepShiftKeysCommand, ShiftKeySweepResult> sweepHandler, IOptionsMonitor<ShiftKeyScannerOptions> options)
    {
        _handler = handler;
        _sweepHandler = sweepHandler;
        _options = options;
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

        var results = await _handler.Handle(new RedeemShiftCodeCommand(requestDto.Code), cancellationToken);

        var total = results.Count;
        var succeeded = results.Count(r => r.Success);
        var summary = new RedeemSummaryDto(total, succeeded, total - succeeded);

        return Ok(new RedeemResponseDto(summary, results));
    }

    /// <summary>
    ///     Scans configured sources for SHiFT keys and redeems any matches found.
    /// </summary>
    /// <param name="hours">Optional lookback window in hours (defaults to configured setting).</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A summary of the sweep and redemption results.</returns>
    [HttpPost("sweep")]
    [ProducesResponseType(typeof(ShiftKeySweepResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Sweep([FromQuery] int? hours, CancellationToken cancellationToken)
    {
        var defaultHours = _options.CurrentValue?.LookbackHours ?? 24;
        var effectiveHours = hours ?? defaultHours;
        var clamped = Math.Clamp(effectiveHours, 1, 168);
        var lookback = TimeSpan.FromHours(clamped);

        var result = await _sweepHandler.Handle(new SweepShiftKeysCommand(lookback), cancellationToken);

        var items = result.Items
                          .Select(item =>
                           {
                               var total = item.Results.Count;
                               var succeeded = item.Results.Count(r => r.Success);
                               var summary = new RedeemSummaryDto(total, succeeded, total - succeeded);

                               return new ShiftKeySweepItemDto(item.Code, item.Sources, summary, item.Results);
                           })
                          .ToList();

        var responseSummary = new ShiftKeySweepSummaryDto(result.Summary.TotalKeys, result.Summary.TotalRedemptionAttempts, result.Summary.TotalSucceeded, result.Summary.TotalFailed);

        return Ok(new ShiftKeySweepResponseDto(result.SinceUtc, result.ScannedAtUtc, responseSummary, items));
    }
}
