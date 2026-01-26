using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Fitbit.Application.Abstractions.Messaging;
using Tomeshelf.Fitbit.Application.Features.Authorization.Commands;
using Tomeshelf.Fitbit.Application.Features.Authorization.Models;
using Tomeshelf.Fitbit.Application.Features.Authorization.Queries;

namespace Tomeshelf.Fitbit.Api.Controllers;

[ApiController]
[Route("api/fitbit/auth")]
public sealed class FitbitAuthorizationController : ControllerBase
{
    private readonly ICommandHandler<BuildFitbitAuthorizationRedirectCommand, FitbitAuthorizationRedirect> _authorizeHandler;
    private readonly ICommandHandler<ExchangeFitbitAuthorizationCodeCommand, FitbitAuthorizationExchangeResult> _exchangeHandler;
    private readonly ILogger<FitbitAuthorizationController> _logger;
    private readonly IQueryHandler<GetFitbitAuthorizationStatusQuery, FitbitAuthorizationStatus> _statusHandler;

    public FitbitAuthorizationController(ICommandHandler<BuildFitbitAuthorizationRedirectCommand, FitbitAuthorizationRedirect> authorizeHandler, ICommandHandler<ExchangeFitbitAuthorizationCodeCommand, FitbitAuthorizationExchangeResult> exchangeHandler, IQueryHandler<GetFitbitAuthorizationStatusQuery, FitbitAuthorizationStatus> statusHandler, ILogger<FitbitAuthorizationController> logger)
    {
        _authorizeHandler = authorizeHandler;
        _exchangeHandler = exchangeHandler;
        _statusHandler = statusHandler;
        _logger = logger;
    }

    [HttpGet("authorize")]
    public async Task<IActionResult> Authorize([FromQuery] string returnUrl)
    {
        var result = await _authorizeHandler.Handle(new BuildFitbitAuthorizationRedirectCommand(returnUrl), CancellationToken.None);

        return Redirect(result.AuthorizationUri.ToString());
    }

    [HttpGet("callback")]
    public async Task<IActionResult> Callback([FromQuery] string code, [FromQuery] string state, [FromQuery] string error, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(error))
        {
            _logger.LogWarning("Fitbit authorization failed with error '{Error}'.", error);

            return BadRequest(new { message = $"Fitbit authorization error: {error}" });
        }

        if (string.IsNullOrWhiteSpace(code))
        {
            return BadRequest(new { message = "Missing authorization code." });
        }

        var exchangeResult = await _exchangeHandler.Handle(new ExchangeFitbitAuthorizationCodeCommand(code, state ?? string.Empty), cancellationToken);
        if (exchangeResult.IsInvalidState)
        {
            return BadRequest(new { message = "Invalid or expired OAuth state token." });
        }

        return Redirect(string.IsNullOrWhiteSpace(exchangeResult.ReturnUrl)
                            ? "/fitness"
                            : exchangeResult.ReturnUrl);
    }

    [HttpGet("status")]
    public async Task<IActionResult> Status()
    {
        var status = await _statusHandler.Handle(new GetFitbitAuthorizationStatusQuery(), CancellationToken.None);

        return Ok(new
        {
            hasAccessToken = status.HasAccessToken,
            hasRefreshToken = status.HasRefreshToken
        });
    }
}
