using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Web.Infrastructure;
using Tomeshelf.Web.Models.DriveAuth;

namespace Tomeshelf.Web.Controllers;

[Route("drive-auth")]
public sealed class DriveAuthController : Controller
{
    private const string TokenEndpoint = "https://oauth2.googleapis.com/token";
    private const string AuthorizeEndpoint = "https://accounts.google.com/o/oauth2/v2/auth";
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<DriveAuthController> _logger;

    public DriveAuthController(IConfiguration configuration, IHttpClientFactory httpClientFactory, ILogger<DriveAuthController> logger)
    {
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    [HttpGet("start")]
    public IActionResult Start([FromQuery] string? returnUrl = null)
    {
        var (clientId, clientSecret, userEmail) = LoadOAuthConfig();
        if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
        {
            return View("OAuthResult", new OAuthResultViewModel
            {
                Success = false,
                Message = "Google Drive OAuth is not configured. Set GoogleDrive:ClientId and GoogleDrive:ClientSecret in AppHost."
            });
        }

        var redirectUri = BuildRedirectUri();
        var state = Guid.NewGuid()
                        .ToString("N");
        HttpContext.Session.SetString(GoogleDriveSessionKeys.OAuthState, state);
        if (!string.IsNullOrWhiteSpace(returnUrl))
        {
            HttpContext.Session.SetString("gd_returnUrl", returnUrl);
        }

        var authUrl = QueryHelpers.AddQueryString(AuthorizeEndpoint, new Dictionary<string, string?>
        {
            ["client_id"] = clientId,
            ["redirect_uri"] = redirectUri,
            ["response_type"] = "code",
            ["scope"] = "https://www.googleapis.com/auth/drive",
            ["access_type"] = "offline",
            ["prompt"] = "consent",
            ["include_granted_scopes"] = "true",
            ["state"] = state,
            ["login_hint"] = userEmail
        });

        return Redirect(authUrl);
    }

    [HttpGet("callback")]
    public async Task<IActionResult> Callback([FromQuery] string? code, [FromQuery] string? state, CancellationToken cancellationToken)
    {
        var expectedState = HttpContext.Session.GetString(GoogleDriveSessionKeys.OAuthState);
        HttpContext.Session.Remove(GoogleDriveSessionKeys.OAuthState);

        if (string.IsNullOrWhiteSpace(code))
        {
            return View("OAuthResult", new OAuthResultViewModel
            {
                Success = false,
                Message = "Missing OAuth code from Google."
            });
        }

        if (string.IsNullOrWhiteSpace(state) || string.IsNullOrWhiteSpace(expectedState) || !string.Equals(state, expectedState, StringComparison.Ordinal))
        {
            return View("OAuthResult", new OAuthResultViewModel
            {
                Success = false,
                Message = "OAuth state verification failed. Please try again."
            });
        }

        var (clientId, clientSecret, userEmail) = LoadOAuthConfig();
        if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
        {
            return View("OAuthResult", new OAuthResultViewModel
            {
                Success = false,
                Message = "Google Drive OAuth is not configured. Set GoogleDrive:ClientId and GoogleDrive:ClientSecret in AppHost."
            });
        }

        var redirectUri = BuildRedirectUri();
        var token = await ExchangeCodeAsync(code, clientId, clientSecret, redirectUri, cancellationToken);
        if (!string.IsNullOrWhiteSpace(token.Error))
        {
            return View("OAuthResult", new OAuthResultViewModel
            {
                Success = false,
                Message = token.Error
            });
        }

        if (string.IsNullOrWhiteSpace(token.RefreshToken))
        {
            return View("OAuthResult", new OAuthResultViewModel
            {
                Success = false,
                Message = "Google did not return a refresh token. Re-run the flow and accept offline access."
            });
        }

        HttpContext.Session.SetString(GoogleDriveSessionKeys.ClientId, clientId);
        HttpContext.Session.SetString(GoogleDriveSessionKeys.ClientSecret, clientSecret);
        HttpContext.Session.SetString(GoogleDriveSessionKeys.RefreshToken, token.RefreshToken);
        if (!string.IsNullOrWhiteSpace(userEmail))
        {
            HttpContext.Session.SetString(GoogleDriveSessionKeys.UserEmail, userEmail);
        }

        var target = HttpContext.Session.GetString("gd_returnUrl") ?? Url.Action("Upload", "Bundles");
        HttpContext.Session.Remove("gd_returnUrl");

        return View("OAuthResult", new OAuthResultViewModel
        {
            Success = true,
            Message = "Google Drive authorised for this browser session. You can now upload bundles.",
            UserEmail = userEmail,
            AccessTokenExpiresIn = token.ExpiresIn,
            RedirectUrl = target
        });
    }

    private (string? ClientId, string? ClientSecret, string? UserEmail) LoadOAuthConfig()
    {
        var drive = _configuration.GetSection("GoogleDrive");

        return (drive["ClientId"], drive["ClientSecret"], drive["UserEmail"]);
    }

    private string BuildRedirectUri()
    {
        return Url.Action("Callback", "DriveAuth", null, Request.Scheme, Request.Host.ToUriComponent()) ?? throw new InvalidOperationException("Unable to determine OAuth redirect URI.");
    }

    private async Task<TokenResponsePayload> ExchangeCodeAsync(string code, string clientId, string clientSecret, string redirectUri, CancellationToken cancellationToken)
    {
        try
        {
            var http = _httpClientFactory.CreateClient();
            using var content = new FormUrlEncodedContent(new Dictionary<string, string?>
            {
                ["code"] = code,
                ["client_id"] = clientId,
                ["client_secret"] = clientSecret,
                ["redirect_uri"] = redirectUri,
                ["grant_type"] = "authorization_code"
            });

            using var response = await http.PostAsync(TokenEndpoint, content, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Google token exchange failed with status {Status}: {Body}", response.StatusCode, body);

                return new TokenResponsePayload { Error = $"Token exchange failed ({(int)response.StatusCode}): {response.ReasonPhrase}" };
            }

            var payload = await response.Content.ReadFromJsonAsync<TokenResponsePayload>(cancellationToken);
            if (payload is null)
            {
                return new TokenResponsePayload { Error = "Google returned an empty token response." };
            }

            return payload;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to exchange Google OAuth code for tokens.");

            return new TokenResponsePayload { Error = $"Unexpected error exchanging code: {ex.Message}" };
        }
    }

    private sealed class TokenResponsePayload
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; init; }

        [JsonPropertyName("refresh_token")]
        public string? RefreshToken { get; init; }

        [JsonPropertyName("expires_in")]
        public int? ExpiresIn { get; init; }

        [JsonPropertyName("error")]
        public string? Error { get; init; }
    }
}