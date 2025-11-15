using System.ComponentModel.DataAnnotations;

namespace Tomeshelf.Application.Options;

/// <summary>
///     Configuration options for accessing the Fitbit Web API.
/// </summary>
public sealed class FitbitOptions
{
    /// <summary>
    ///     Gets or sets the Fitbit OAuth 2.0 client identifier.
    /// </summary>
    [Required]
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the Fitbit OAuth 2.0 client secret.
    /// </summary>
    [Required]
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets an optional bootstrap access token. When omitted the application will request one via OAuth.
    /// </summary>
    public string AccessToken { get; set; }

    /// <summary>
    ///     Gets or sets an optional bootstrap refresh token. When omitted the application will request one via OAuth.
    /// </summary>
    public string RefreshToken { get; set; }

    /// <summary>
    ///     Gets or sets the base URL for Fitbit API requests. Defaults to https://api.fitbit.com/ when unspecified.
    /// </summary>
    [Url]
    public string ApiBase { get; set; }

    /// <summary>
    ///     Gets or sets the user identifier. Use "-" to address the currently authenticated user.
    /// </summary>
    public string UserId { get; set; } = "-";

    /// <summary>
    ///     Gets or sets the OAuth 2.0 scopes requested during authorization.
    /// </summary>
    public string Scope { get; set; } = "activity nutrition sleep weight profile settings";

    /// <summary>
    ///     Gets or sets the absolute base URI used to construct the OAuth 2.0 callback.
    /// </summary>
    [Url]
    public string CallbackBaseUri { get; set; }

    /// <summary>
    ///     Gets or sets the relative callback path Fitbit redirects to after authorization.
    /// </summary>
    public string CallbackPath { get; set; } = "/api/fitbit/auth/callback";
}