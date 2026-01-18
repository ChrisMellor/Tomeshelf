using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Infrastructure.Fitness;

namespace Tomeshelf.Fitbit.Api.Controllers;

[ApiController]
[Route("api/fitbit/auth")]
public sealed class FitbitAuthorizationController : ControllerBase
{
    private readonly FitbitAuthorizationService _authorizationService;
    private readonly ILogger<FitbitAuthorizationController> _logger;
    private readonly FitbitTokenCache _tokenCache;

    public FitbitAuthorizationController(FitbitAuthorizationService authorizationService, FitbitTokenCache tokenCache, ILogger<FitbitAuthorizationController> logger)
    {
        _authorizationService = authorizationService;
        _tokenCache = tokenCache;
        _logger = logger;
    }

    [HttpGet("authorize")]
    public IActionResult Authorize([FromQuery] string returnUrl)
    {
        var uri = _authorizationService.BuildAuthorizationUri(returnUrl, out _);

        return Redirect(uri.ToString());
    }

    [HttpGet("callback")]
    public async Task<IActionResult> Callback([FromQuery] string code, [FromQuery] string state, [FromQuery] string error, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(error))
        {
            _logger.LogWarning("Fitbit authorization failed with error '{Error}'.", error);

            return BadRequest(new
            {
                message = $"Fitbit authorization error: {error}"
            });
        }

        if (string.IsNullOrWhiteSpace(code))
        {
            return BadRequest(new
            {
                message = "Missing authorization code."
            });
        }

        if (!_authorizationService.TryConsumeState(state ?? string.Empty, out var codeVerifier, out var returnUrl))
        {
            return BadRequest(new
            {
                message = "Invalid or expired OAuth state token."
            });
        }

        await _authorizationService.ExchangeAuthorizationCodeAsync(code, codeVerifier, cancellationToken);

        return Redirect(string.IsNullOrWhiteSpace(returnUrl)
                            ? "/fitness"
                            : returnUrl);
    }

    [HttpGet("status")]
    public IActionResult Status()
    {
        var hasAccess = !string.IsNullOrWhiteSpace(_tokenCache.AccessToken);
        var hasRefresh = !string.IsNullOrWhiteSpace(_tokenCache.RefreshToken);

        return Ok(new
        {
            hasAccessToken = hasAccess,
            hasRefreshToken = hasRefresh
        });
    }
}