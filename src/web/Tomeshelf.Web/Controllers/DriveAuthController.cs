using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Tomeshelf.Web.Infrastructure;
using Tomeshelf.Web.Models.DriveAuth;

namespace Tomeshelf.Web.Controllers;

[Route("drive-auth")]
public sealed class DriveAuthController : Controller
{
    public const string AuthenticationScheme = "GoogleDrive";
    private readonly IConfiguration _configuration;

    public DriveAuthController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpGet("result")]
    public IActionResult Result([FromQuery] string? returnUrl = null)
    {
        var error = HttpContext.Session.GetString(GoogleDriveSessionKeys.Error);
        if (!string.IsNullOrWhiteSpace(error))
        {
            HttpContext.Session.Remove(GoogleDriveSessionKeys.Error);

            return View("OAuthResult", new OAuthResultViewModel
            {
                Success = false,
                Message = error
            });
        }

        var hasTokens = HasDriveTokens();
        if (!hasTokens)
        {
            return View("OAuthResult", new OAuthResultViewModel
            {
                Success = false,
                Message = "Google authorisation did not complete. Please try again."
            });
        }

        var userEmail = HttpContext.Session.GetString(GoogleDriveSessionKeys.UserEmail);
        var target = returnUrl ?? HttpContext.Session.GetString(GoogleDriveSessionKeys.ReturnUrl) ?? Url.Action("Upload", "Bundles");
        HttpContext.Session.Remove(GoogleDriveSessionKeys.ReturnUrl);

        return View("OAuthResult", new OAuthResultViewModel
        {
            Success = true,
            Message = "Google Drive authorised for this browser session. You can now upload bundles.",
            UserEmail = userEmail,
            RedirectUrl = target
        });
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

        if (!string.IsNullOrWhiteSpace(returnUrl))
        {
            HttpContext.Session.SetString(GoogleDriveSessionKeys.ReturnUrl, returnUrl);
        }

        var properties = new AuthenticationProperties { RedirectUri = Url.Action("Result", "DriveAuth", new { returnUrl }) ?? "/drive-auth/result" };

        if (!string.IsNullOrWhiteSpace(userEmail))
        {
            properties.Items["login_hint"] = userEmail;
        }

        return Challenge(properties, AuthenticationScheme);
    }

    private bool HasDriveTokens()
    {
        return !string.IsNullOrWhiteSpace(HttpContext.Session.GetString(GoogleDriveSessionKeys.ClientId)) && !string.IsNullOrWhiteSpace(HttpContext.Session.GetString(GoogleDriveSessionKeys.ClientSecret)) && !string.IsNullOrWhiteSpace(HttpContext.Session.GetString(GoogleDriveSessionKeys.RefreshToken));
    }

    private (string? ClientId, string? ClientSecret, string? UserEmail) LoadOAuthConfig()
    {
        var drive = _configuration.GetSection("GoogleDrive");

        return (drive["ClientId"], drive["ClientSecret"], drive["UserEmail"]);
    }
}