using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Tomeshelf.Application.Shared.Abstractions.Messaging;
using Tomeshelf.Fitbit.Application.Features.Authorization.Models;
using Tomeshelf.Fitbit.Application.Features.Authorization.Queries;

namespace Tomeshelf.Fitbit.Api.Controllers;

[ApiController]
[Route("api/fitbit/auth")]
public sealed class FitbitAuthorizationController : ControllerBase
{
    private readonly ILogger<FitbitAuthorizationController> _logger;
    private readonly IQueryHandler<GetFitbitAuthorizationStatusQuery, FitbitAuthorizationStatus> _statusHandler;

    public FitbitAuthorizationController(IQueryHandler<GetFitbitAuthorizationStatusQuery, FitbitAuthorizationStatus> statusHandler, ILogger<FitbitAuthorizationController> logger)
    {
        _statusHandler = statusHandler;
        _logger = logger;
    }

    [HttpGet("authorize")]
    public IActionResult Authorize([FromQuery] string returnUrl)
    {
        var target = string.IsNullOrWhiteSpace(returnUrl)
            ? "/fitness"
            : returnUrl;

        var properties = new AuthenticationProperties { RedirectUri = target };

        return Challenge(properties, FitbitOAuthDefaults.AuthenticationScheme);
    }

    [HttpGet("failure")]
    public IActionResult Failure([FromQuery] string message)
    {
        var payload = string.IsNullOrWhiteSpace(message)
            ? "Fitbit authorisation failed."
            : message;

        _logger.LogWarning("Fitbit authorisation failure: {Message}", payload);

        return BadRequest(new { message = payload });
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